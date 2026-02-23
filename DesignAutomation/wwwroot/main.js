let viewer;

document.addEventListener("DOMContentLoaded", function () {
    loadFiles();
    document.getElementById('bucketFiles').addEventListener('change', (e) => {
        if (e.target.value) launchViewer(e.target.value);
    });

    document.getElementById('fileInput').addEventListener('change', uploadFile);

    document.getElementById('btnDownload').addEventListener('click', downloadFile);
});

async function loadFiles() {
    const resp = await fetch('/api/oss/files');
    const files = await resp.json();
    const select = document.getElementById('bucketFiles');
    select.innerHTML = '<option value="">-- Chọn file để xem --</option>';
    files.forEach(f => {
        const opt = document.createElement('option');
        opt.value = f.urn;
        opt.textContent = f.fileName;
        select.appendChild(opt);
    });
}

async function uploadFile(e) {
    const file = e.target.files[0];
    if (!file) return;
    const formData = new FormData();
    formData.append('file', file);
    await fetch('/api/oss/upload', { method: 'POST', body: formData });
    await loadFiles();
}

async function downloadFile() {
    const select = document.getElementById('bucketFiles');
    const fileName = select.options[select.selectedIndex].text;

    if (!fileName || fileName.includes("--")) {
        alert("Vui lòng chọn một file trong danh sách!");
        return;
    }
    const apiPath = `/api/oss/download/${encodeURIComponent(fileName)}`;  
    window.location.href = apiPath;
}

function launchViewer(urn) {
    const options = {
        env: 'AutodeskProduction',
        getAccessToken: async (callback) => {
            const resp = await fetch('/api/auth/token');
            const data = await resp.json();
            callback(data.access_token, data.expires_in);
        }
    };

    document.getElementById('loader').classList.remove('hidden');

    Autodesk.Viewing.Initializer(options, () => {
        const container = document.getElementById('forgeViewer');
        if (viewer) viewer.finish();
        viewer = new Autodesk.Viewing.GuiViewer3D(container);
        viewer.start();
        Autodesk.Viewing.Document.load('urn:' + urn,
            (doc) => {
                document.getElementById('loader').classList.add('hidden');
                viewer.loadDocumentNode(doc, doc.getRoot().getDefaultGeometry());
            },
            (err) => {
                handleTranslation(urn);
            }
        );
    });
}

async function handleTranslation(urn) {
    await fetch(`/api/modelderivative/translate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ urn: urn })
    });
}