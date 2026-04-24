# chain.py
import fitz
import pdfplumber
from handlers import BaseHandler

class ParsingChain:
    def __init__(self, handlers: list[BaseHandler]):
        self.handlers = handlers

    def process(self, pdf_path: str, output_dir: str) -> dict:
        # fitz_doc = fitz.open(pdf_path)
        with pdfplumber.open(pdf_path) as pdf:
            fitz_doc = fitz.open(pdf_path)
            results = {}
            for page_num in range(1, fitz_doc.page_count + 1):
                fitz_page = fitz_doc.load_page(page_num - 1)
                plumber_page = pdf.pages[page_num - 1]

                context = {
                    "fitz_page": fitz_page,
                    "plumber_page": plumber_page,
                    "fitz_doc": fitz_doc,
                }

                for handler in self.handlers:
                    if handler.can_handle(context):
                        res = handler.handle(context, page_num, output_dir)
                        if res is not None:
                            results[page_num] = {
                                "handler": type(handler).__name__,
                                "result": res
                            }
                            break
                else:
                    raise RuntimeError(f"Не удалось обработать страницу {page_num}")
        return results