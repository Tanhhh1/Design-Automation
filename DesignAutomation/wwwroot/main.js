let viewer;

document.addEventListener("DOMContentLoaded", function () {
    loadFiles();
    document.getElementById('bucketFiles').addEventListener('change', (e) => {
        if (e.target.value) launchViewer(e.target.value);
    });

    document.getElementById('fileInput').addEventListener('change', uploadFile);

    document.getElementById('btnDownload').addEventListener('click', downloadFile);

    document.getElementById('btnExportExcel').addEventListener('click', exportAllToExcel);

    document.getElementById('btnExportSelection').addEventListener('click', exportSelectionToExcel);
});

function safeBase64(str) {
    return btoa(str).replace(/\//g, '_').replace(/\+/g, '-').replace(/=/g, '');
}

async function loadFiles() {
    const resp = await fetch('/api/oss/files');
    const files = await resp.json();
    const select = document.getElementById('bucketFiles');
    select.innerHTML = '<option value="">-- Chọn file để xem --</option>';
    files.forEach(f => {
        const opt = document.createElement('option');
        opt.value = safeBase64(f.objectId);
        opt.textContent = f.objectKey;
        select.appendChild(opt);
    });
}

async function uploadFile(e) {
    /*const file = e.target.files[0];
    if (!file) return;
    const formData = new FormData();
    formData.append('file', file); //đóng gói file vào formData
    await fetch('/api/oss/upload', { method: 'POST', body: formData }); //gửi file đến /api/oss/upload
    await loadFiles();*/

    const file = e.target.files[0];
    if (!file) return;
    const resp = await fetch(`/api/oss/upload-url?fileName=${encodeURIComponent(file.name)}`); //lấy signed URL từ server
    const data = await resp.json();
    const signedUrl = data.uploadUrl; // Đây là địa chỉ trực tiếp dẫn đến Cloud của Autodesk
    const uploadResp = await fetch(signedUrl, {
        method: 'PUT', // Dùng PUT để đặt file vào vị trí đã định danh
        body: file, // Gửi trực tiếp dữ liệu nhị phân (không đóng gói FormData)
        headers: {
            'Content-Type': 'application/octet-stream' // Báo hiệu đây là dữ liệu thô
        }
    });
    if (uploadResp.ok) {
        alert("Upload trực tiếp thành công!");
        await loadFiles();
    } else {
        alert("Upload thất bại!");
    }
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


async function exportAllToExcel() {
    if (!viewer || !viewer.model) return;
    const tree = viewer.model.getInstanceTree();
    const allDbIds = [];
    if (tree) {
        const rootId = tree.getRootId();
        tree.enumNodeChildren(rootId, (dbId) => {
            allDbIds.push(dbId);
        }, true);
    } else {
        const lastId = viewer.model.getData().instanceCount;
        for (let i = 1; i <= lastId; i++) {
            allDbIds.push(i);
        }
    }
    viewer.model.getBulkProperties(allDbIds, null, (elements) => {
        const validElements = elements.filter(el =>
            el.properties && el.properties.some(p => p.displayName === "Category")
        );

        if (validElements.length > 0) {
            executeExport(validElements, "Revit_Full_Export");
        }
    });
}

async function exportSelectionToExcel() {
    const selection = viewer.getSelection();
    if (selection.length === 0) return;

    viewer.model.getBulkProperties(selection, null, (elements) => {
        executeExport(elements, "Revit_Selection_Export");
    });
}

async function executeExport(data, fileNamePrefix) {
    try {
        const payload = data.map(el => ({
            dbId: el.dbId,
            name: el.name,
            properties: el.properties.map(p => ({
                displayName: p.displayName,
                displayValue: p.displayValue
            }))
        }));

        const response = await fetch('/api/oss/excel', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (!response.ok) return;

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.style.display = 'none';
        a.href = url;
        a.download = `${fileNamePrefix}_${new Date().getTime()}.xlsx`;
        document.body.appendChild(a);
        a.click();

        setTimeout(() => {
            window.URL.revokeObjectURL(url);
            a.remove();
        }, 100);

    } catch (err) {
        console.error("Export Error:", err);
    }
}