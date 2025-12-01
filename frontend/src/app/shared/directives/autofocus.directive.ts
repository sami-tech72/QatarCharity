import { AfterViewInit, Directive, ElementRef } from '@angular/core';

@Directive({
  selector: '[appAutofocus]',
  standalone: true,
})
export class AutofocusDirective implements AfterViewInit {
  constructor(private readonly elementRef: ElementRef<HTMLInputElement>) {}

  ngAfterViewInit(): void {
    this.elementRef.nativeElement?.focus?.();
  }
}
