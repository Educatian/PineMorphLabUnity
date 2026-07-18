mergeInto(LibraryManager.library, {
  PineMorphDownload: function (fileNamePtr, contentPtr) {
    var fileName = UTF8ToString(fileNamePtr);
    var content = UTF8ToString(contentPtr);
    var blob = new Blob([content], { type: "text/csv;charset=utf-8" });
    var link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    setTimeout(function () { URL.revokeObjectURL(link.href); }, 1000);
  },
  PineMorphEmitEvent: function (jsonPtr) {
    var detail = JSON.parse(UTF8ToString(jsonPtr));
    window.dispatchEvent(new CustomEvent("pinemorph-learning-event", { detail: detail }));
    window.postMessage({ source: "pinemorph-lab", detail: detail }, window.location.origin);
  }
});
