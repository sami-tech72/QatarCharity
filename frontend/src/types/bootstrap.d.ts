declare module 'bootstrap' {
  export interface ModalOptions {
    backdrop?: boolean | 'static';
    focus?: boolean;
    keyboard?: boolean;
  }

  export class Modal {
    constructor(element: Element, options?: ModalOptions);
    static getInstance(element: Element): Modal | null;
    static getOrCreateInstance(element: Element, options?: ModalOptions): Modal;
    show(): void;
    hide(): void;
    toggle(): void;
  }
}
