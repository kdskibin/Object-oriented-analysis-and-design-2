import os
import uuid
from flask import Flask, render_template, request, redirect
from werkzeug.utils import secure_filename
from chain import ParsingChain
from handlers import SimpleTextHandler, IntermediateHandler, ImageOnlyHandler
import hashlib
import logging
from pathlib import Path

# Конфигурация
BASE_DIR = os.path.abspath(os.path.dirname(__file__))
UPLOAD_FOLDER = os.path.join(BASE_DIR, 'source_docs')
ALLOWED_EXTENSIONS = {'pdf'}

app = Flask(__name__)
app.config['UPLOAD_FOLDER'] = UPLOAD_FOLDER
app.config['MAX_CONTENT_LENGTH'] = 64 * 1024 * 1024 # 64 МБ максимум

handlers = [
    SimpleTextHandler(),
    IntermediateHandler(min_text_length=50, max_images_for_skip=10),
    ImageOnlyHandler(resolution=300)
]
chain = ParsingChain(handlers)


def compute_file_sha256(file_path: Path, chunk_size: int = 1024 * 1024) -> str:
    h = hashlib.sha256()
    with open(file_path, "rb") as f:
        while True:
            chunk = f.read(chunk_size)
            if not chunk:
                break
            h.update(chunk)
    return h.hexdigest()


def allowed_file(filename):
    return '.' in filename and filename.rsplit('.', 1)[1].lower() in ALLOWED_EXTENSIONS


@app.route('/')
def index():
    return render_template('index.html')


@app.route('/upload', methods=['POST'])
def upload():
    if 'pdf_file' not in request.files:
        return redirect(request.url)
    file = request.files['pdf_file']
    if file.filename == '':
        return redirect(request.url)
    if file and allowed_file(file.filename):
        # Генерируем уникальный идентификатор для запроса
        request_id = str(uuid.uuid4())
        # Сохраняем загруженный PDF
        os.makedirs(app.config['UPLOAD_FOLDER'], exist_ok=True)
        filename = secure_filename(file.filename)
        pdf_path = os.path.join(app.config['UPLOAD_FOLDER'], filename)
        file.save(pdf_path)

        output_dir = os.path.join(BASE_DIR, 'static', 'results', request_id)

        try:
            # Запускаем цепочку обработки
            results = chain.process(pdf_path, output_dir)
        except Exception as e:
            # Ошибка: покажем на главной странице
            error_msg = f"Ошибка при обработке PDF: {str(e)}"
            return render_template('index.html', error=error_msg)

        # Преобразуем полные пути в относительные для доступа через /static
        pages_info = []
        for page_num, data in sorted(results.items()):
            handler_name = data['handler']
            res = data['result']
            rel_root = os.path.relpath(output_dir, os.path.join(BASE_DIR, 'static'))

            text_rel = None
            if 'text_path' in res:
                text_rel = os.path.relpath(res['text_path'], output_dir)

            tables_rel = []
            if 'tables' in res:
                for t_path in res['tables']:
                    tables_rel.append(os.path.relpath(t_path, output_dir))

            images_rel = []
            if 'images' in res:
                for i_path in res['images']:
                    images_rel.append(os.path.relpath(i_path, output_dir))

            snapshot_rel = None
            if 'snapshot_path' in res:
                snapshot_rel = os.path.relpath(res['snapshot_path'], output_dir)

            pages_info.append({
                'page': page_num,
                'handler': handler_name,
                'text_rel': text_rel,
                'tables_rel': tables_rel,
                'images_rel': images_rel,
                'snapshot_rel': snapshot_rel
            })

        return render_template('result.html',
                               request_id=request_id,
                               pages=pages_info,
                               title=f"Результат обработки: {filename}")
    else:
        return render_template('index.html', error="Разрешены только файлы PDF")


if __name__ == '__main__':
    app.run(debug=True)