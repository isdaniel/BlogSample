const CHUNK_SIZE = 10 * 1024 * 1024; // 10MB per chunk (adjust as needed)

document.getElementById('uploadButton').addEventListener('click', async () => {
    const file = document.getElementById('fileInput').files[0];
    if (file) {
        await uploadFileInChunks(file);
    }
});

async function uploadFileInChunks(file) {
    const totalChunks = Math.ceil(file.size / CHUNK_SIZE);
    let uploadCnt = 0;
    // Helper function to upload each chunk
    const uploadChunk = (chunk, index) => {
        return new Promise((resolve, reject) => {
            const formData = new FormData();
            formData.append('FileChunk', chunk);
            formData.append('Index', index);
            formData.append('TotalChunks', totalChunks);

            fetch('/Home/Upload', {
                method: 'POST',
                body: formData
            })
            .then(response => {
                console.log(response.text());
            })
            .then(resolve)
            .catch(reject);
        });
    };

    // Loop through the file in chunks
    for (let start = 0; start < file.size; start += CHUNK_SIZE) {
        const chunkReader = new FileReader();
        const end = Math.min(start + CHUNK_SIZE, file.size);
        const chunk = file.slice(start, end);
        chunkReader.onloadend = async function () {
            const index = Math.floor(start / CHUNK_SIZE);
            console.log(`Uploading chunk ${index + 1} of ${totalChunks}`);
            await uploadChunk(chunk, index);
            uploadCnt++;
            updateProgress(uploadCnt / totalChunks);
        };

        chunkReader.readAsArrayBuffer(chunk);
    }
}

function updateProgress(progress) {
    const progressBar = document.getElementById('progress');
    progressBar.innerHTML = `Upload Progress: ${(progress * 100).toFixed(2)}%`;
    if (progress == 1) {
        alert('File upload complete!!');
    }
}