# handlers/base_handler.py
from abc import ABC, abstractmethod
from typing import Optional, Dict, Any, Union
import os
import re
import logging
import pandas as pd
import numpy as np
import pdfplumber


def is_not_enumeration(row: list) -> bool:
    """Отфильтровывает строки, состоящие только из порядковых номеров"""

    pattern = [str(i) for i in range(1, len(row) + 1)]
    return row != pattern


def clean_cell(content: Union[str, None]) -> str:
    if content is None:
        return 'n/a'
    return str(content).replace('-\n', '').replace('\n', ' ').strip()


def extract_caption(page, bbox: tuple, y_margin: float = 40, x_margin: float = 40) -> str:
    """Извлекает текст рядом с bbox, который может быть подписью"""
    
    bx0, btop, bx1, bbottom = bbox
    words = page.extract_words(x_tolerance=3, y_tolerance=3)
    caption_parts = []
    
    for w in words:
        wy_center = (w['top'] + w['bottom']) / 2
        wx_center = (w['x0'] + w['x1']) / 2
        # Проверяем, находится ли слово рядом по Y или по X, но не внутри самого элемента
        y_near = (btop - y_margin <= wy_center <= bbottom + y_margin)
        x_near = (bx0 - x_margin <= wx_center <= bx1 + x_margin)
        not_inside = not (btop <= wy_center <= bbottom and bx0 <= wx_center <= bx1)
        
        if y_near and x_near and not_inside:
            caption_parts.append(w['text'])
            
    return ' '.join(caption_parts).strip()


class BaseHandler(ABC):
    @abstractmethod
    def can_handle(self, context: Dict[str, Any]) -> bool:
        """
        context содержит:
            - "fitz_page": fitz.Page
            - "plumber_page": pdfplumber.page.Page
            - "fitz_doc": fitz.Document
        """
        pass

    @abstractmethod
    def handle(self, context: Dict[str, Any], page_num: int, output_dir: str) -> Optional[Dict[str, Any]]:
        pass


class SimpleTextHandler(BaseHandler):
    def can_handle(self, context) -> bool:
        plumber_page = context["plumber_page"]
        fitz_page = context["fitz_page"]

        # Проверяем изображения через pdfplumber
        if plumber_page.images:
            return False

        # Текст получаем через fitz
        text = fitz_page.get_text()
        if "(cid:" in text:          # маркеры повреждённых символов
            return False
        return bool(text.strip())

    def handle(self, context, page_num: int, output_dir: str) -> dict:
        fitz_page = context["fitz_page"]
        text = fitz_page.get_text()

        texts_dir = os.path.join(output_dir, "texts")
        os.makedirs(texts_dir, exist_ok=True)
        path = os.path.join(texts_dir, f"{page_num}.txt")
        with open(path, "w", encoding="utf-8") as f:
            f.write(text)
        return {"text": text, "text_path": path}


class IntermediateHandler(BaseHandler):
    def __init__(self, min_text_length: int = 50, max_images_for_skip: int = 10):
        self.min_text_length = min_text_length
        self.max_images_for_skip = max_images_for_skip

    def can_handle(self, context) -> bool:
        plumber_page = context["plumber_page"]
        imgs = plumber_page.images
        text = plumber_page.extract_text() or ""
        # Слишком много картинок и почти нет текста -> пропускаем
        if len(imgs) > self.max_images_for_skip and len(text) < 100:
            return False
        return True

    def handle(self, context, page_num: int, output_dir: str) -> dict | None:
        try:
            page = context["plumber_page"] # pdfplumber-страница
            images_margin = 18

            table_objects = page.find_tables()
            table_bboxes = [t.bbox for t in table_objects]
            raw_images = page.images
            image_bboxes = [
                (
                    np.clip(img['x0'] - images_margin, 0, page.width),
                    np.clip(img['top'] - images_margin, 0, page.height),
                    np.clip(img['x1'] + images_margin, 0, page.width),
                    np.clip(img['bottom'] + images_margin, 0, page.height)
                )
                for img in raw_images
            ]

            events = []
            words = page.extract_words(x_tolerance=5, y_tolerance=3)
            for w in words:
                events.append({'type': 'text', 'top': w['top'], 'text': w['text']})
            for i, bbox in enumerate(table_bboxes):
                events.append({'type': 'table', 'top': bbox[1], 'index': i, 'bbox': bbox})
            for i, bbox in enumerate(image_bboxes):
                events.append({
                    'type': 'image', 'top': bbox[1], 'index': i, 'bbox': bbox,
                    'orig_bbox': (raw_images[i]['x0'], raw_images[i]['top'],
                                  raw_images[i]['x1'], raw_images[i]['bottom'])
                })

            events.sort(key=lambda x: x['top'])

            page_text_parts = []
            captions = {}
            for ev in events:
                if ev['type'] == 'text':
                    page_text_parts.append(ev['text'])
                elif ev['type'] == 'table':
                    caption = extract_caption(page, ev["bbox"])
                    captions[ev["bbox"]] = caption
                    page_text_parts.append(f"\n[TABLE]; [CAPTION: {caption}]\n")
                elif ev['type'] == 'image':
                    caption = extract_caption(page, ev["bbox"])
                    captions[ev["bbox"]] = caption
                    page_text_parts.append(f"\n[IMAGE]; [CAPTION: {caption}]\n")

            page_text = re.sub(r'\s+', ' ', ' '.join(page_text_parts)).strip()

            # Таблицы
            tables_data = []
            tables = page.extract_tables()
            for i, t in enumerate(tables):
                table = [row for row in t if is_not_enumeration(row)]
                table = [[clean_cell(el) for el in row] for row in table]
                df = pd.DataFrame(table).replace('n/a', np.nan).replace('', 'n/a')
                bbox = table_bboxes[i]
                caption = captions.get(bbox, extract_caption(page, bbox))
                img_obj = page.crop(bbox).to_image(resolution=600).original
                tables_data.append({'df': df, 'image': img_obj, 'caption': caption})

            # Изображения
            images_data = []
            for i, img in enumerate(raw_images):
                extended_bbox = image_bboxes[i]
                caption = captions.get(extended_bbox, extract_caption(page, extended_bbox))
                try:
                    img_obj = page.crop(extended_bbox).to_image(resolution=600).original
                    images_data.append({
                        'image': img_obj,
                        'caption': caption,
                        'orig_bbox': (img['x0'], img['top'], img['x1'], img['bottom'])
                    })
                except Exception as e:
                    logging.warning(f"Image extraction failed on page {page_num}: {e}")

            # Отказ, если итоговый текст слишком короток и таблиц нет
            if len(page_text) < self.min_text_length and not tables_data:
                return None

            # Сохраняем результаты
            texts_dir = os.path.join(output_dir, "texts")
            tables_dir = os.path.join(output_dir, "tables")
            images_dir = os.path.join(output_dir, "images")
            for d in (texts_dir, tables_dir, images_dir):
                os.makedirs(d, exist_ok=True)

            text_path = os.path.join(texts_dir, f"{page_num}.txt")
            with open(text_path, "w", encoding="utf-8") as f:
                f.write(page_text)

            table_paths = []
            for j, td in enumerate(tables_data):
                fname = f"{page_num}_{j+1}.png"
                img_path = os.path.join(tables_dir, fname)
                td['image'].save(img_path)
                csv_path = os.path.join(tables_dir, f"{page_num}_{j+1}.csv")
                td['df'].to_csv(csv_path, index=False)
                table_paths.append(img_path)

            image_paths = []
            for k, idata in enumerate(images_data):
                fname = f"{page_num}_{k+1}.png"
                img_path = os.path.join(images_dir, fname)
                idata['image'].save(img_path)
                image_paths.append(img_path)

            return {
                "text": page_text,
                "text_path": text_path,
                "tables": table_paths,
                "images": image_paths
            }

        except Exception as e:
            logging.error(f"IntermediateHandler failed on page {page_num}: {e}")
            return None


class ImageOnlyHandler(BaseHandler):
    def __init__(self, resolution: int = 300):
        self.resolution = resolution

    def can_handle(self, context) -> bool:
        return True   # fallback для любой страницы

    def handle(self, context, page_num: int, output_dir: str) -> dict:
        # Используем pdfplumber-страницу для рендеринга
        plumber_page = context["plumber_page"]
        snap_dir = os.path.join(output_dir, "page_snapshots")
        os.makedirs(snap_dir, exist_ok=True)
        img = plumber_page.to_image(resolution=self.resolution).original
        path = os.path.join(snap_dir, f"{page_num}.png")
        img.save(path)
        return {"snapshot_path": path}