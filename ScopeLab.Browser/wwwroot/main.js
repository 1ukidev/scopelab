const overlay = document.getElementById('overlay');
const status = document.getElementById('status');
const start = document.getElementById('start');
const progress = document.getElementById('progress');
const progressBar = document.getElementById('progress-bar');

const strings = navigator.language.startsWith('pt')
    ? {
        warning: size => `O ScopeLab precisa baixar até ${size} de arquivos para rodar no navegador.`,
        warningUnknown: 'O ScopeLab precisa baixar seus arquivos para rodar no navegador.',
        start: 'Baixar e iniciar',
        downloading: 'Baixando...',
        failed: 'Não foi possível iniciar o ScopeLab',
    }
    : {
        warning: size => `ScopeLab needs to download up to ${size} of files to run in the browser.`,
        warningUnknown: 'ScopeLab needs to download its files to run in the browser.',
        start: 'Download and start',
        downloading: 'Downloading...',
        failed: 'ScopeLab failed to start',
    };

// Written when publishing (see ScopeLab.Browser.csproj).
const downloadSize = await fetch('download-size.json')
    .then(response => response.ok ? response.json() : null)
    .then(json => json?.bytes)
    .catch(() => null);

status.textContent = downloadSize
    ? strings.warning(`${Math.ceil(downloadSize / 1024 / 1024)} MB`)
    : strings.warningUnknown;

start.textContent = strings.start;
start.hidden = false;

// The files are large, so they should only be downloaded once the user has confirmed.
await new Promise(resolve => start.addEventListener('click', resolve, { once: true }));

start.hidden = true;
progress.hidden = false;
status.textContent = strings.downloading;

try {
    // The runtime is imported dynamically, as a static import would begin downloading it before confirmation.
    const { dotnet } = await import('./_framework/dotnet.js');

    const runtime = await dotnet
        .withConfig({
            onDownloadResourceProgress: (loaded, total) => {
                progressBar.style.width = `${Math.round(loaded / total * 100)}%`;
            },
        })
        .create();

    overlay.hidden = true;
    document.getElementById('canvas').focus();

    // Note that the runtime must not be exited after Main() returns, as the game keeps running via the browser's event loop.
    await runtime.runMain();
} catch (e) {
    overlay.hidden = false;
    progress.hidden = true;
    status.textContent = `${strings.failed}: ${e}`;
    throw e;
}
