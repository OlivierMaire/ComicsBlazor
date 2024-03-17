// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function showPrompt(message) {
  return prompt(message, 'Type anything here');
}

export var comic = {
  dotNetReference: undefined,
  init(pDotNetReference) {
    console.log("initialize interop...");
    this.dotNetReference = pDotNetReference;
    window.addEventListener("resize", this.reportWindowSize.bind(this));
  },
  reportWindowSize(e) {
    if (this.dotNetReference) {
      this.dotNetReference.invokeMethodAsync('OnResizeFromJs',
        window.innerWidth, window.innerHeight);
    }

  }
}