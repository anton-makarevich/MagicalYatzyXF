import { setProgress, setStatus, showError } from "./splash.js";

const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error(`Expected to be running in a browser`);

try {
    const { dotnet } = await import('./_framework/dotnet.js');

    const dotnetRuntime = await dotnet
        .withDiagnosticTracing(false)
        .withApplicationArgumentsFromQuery()
        .withModuleConfig({
            onDownloadResourceProgress: (loaded, total) => {
                setProgress(loaded, total);
                if (total > 0) {
                    setStatus(`Downloading application (${loaded} of ${total} files)…`);
                }
            }
        })
        .create();

    const config = dotnetRuntime.getConfig();
    setProgress(100, 100);
    setStatus("Starting MagicalYatzy…");
    try {
        await dotnetRuntime.runMain(config.mainAssemblyName, [globalThis.location.href]);
    } catch (error) {
        console.error(error);
        showError("Failed to load MagicalYatzy. Check your connection and reload the page.", "Reload", "./");
    }
} catch (error) {
    console.error(error);
    showError("Failed to load MagicalYatzy. Check your connection and reload the page.", "Reload", "./");
}
