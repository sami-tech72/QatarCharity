const popperGlobal = (globalThis as { Popper?: { createPopper: unknown; createPopperLite?: unknown } }).Popper;

if (!popperGlobal || typeof popperGlobal.createPopper !== 'function') {
  throw new Error('Popper is not available on the window object. Ensure plugins.bundle.js loads before @popperjs/core.');
}

export const { createPopper, createPopperLite } = popperGlobal;
export default popperGlobal;
