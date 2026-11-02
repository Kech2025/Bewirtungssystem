window.downloadFileFromBytes = function (dateiname, mimeType, bytes) {
    const blob = new Blob([new Uint8Array(bytes)], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = dateiname;
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);
    URL.revokeObjectURL(url);
};

window.openPdfInNewWindow = function (bytes, dateiname) {
    const blob = new Blob([new Uint8Array(bytes)], { type: "application/pdf" });
    const url = URL.createObjectURL(blob);

    // Versuch, das PDF in einem neuen Tab zu öffnen.
    const neuesFenster = window.open(url, "_blank");

    if (!neuesFenster || neuesFenster.closed || typeof neuesFenster.closed === "undefined") {
        // Popup wurde blockiert -> stattdessen als Datei herunterladen.
        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = dateiname || "Preisliste.pdf";
        document.body.appendChild(anchor);
        anchor.click();
        document.body.removeChild(anchor);
        setTimeout(function () {
            URL.revokeObjectURL(url);
        }, 60000);
        return false;
    }

    // Blob-URL nach kurzer Zeit freigeben (neues Fenster hat es dann geladen).
    setTimeout(function () {
        URL.revokeObjectURL(url);
    }, 60000);
    return true;
};

window.printHtml = function (html) {
    const frame = document.createElement("iframe");
    frame.style.position = "fixed";
    frame.style.right = "0";
    frame.style.bottom = "0";
    frame.style.width = "0";
    frame.style.height = "0";
    frame.style.border = "0";
    document.body.appendChild(frame);

    const doc = frame.contentWindow.document;
    doc.open();
    doc.write(html);
    doc.close();

    frame.contentWindow.focus();
    frame.contentWindow.onafterprint = function () {
        document.body.removeChild(frame);
    };
    setTimeout(function () {
        frame.contentWindow.print();
    }, 200);
};
