import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'appDateFormat',
  standalone: true,
})
export class DateFormatPipe implements PipeTransform {
  transform(value: string | Date, locale = 'en-US', options?: Intl.DateTimeFormatOptions): string {
    const date = value instanceof Date ? value : new Date(value);
    return new Intl.DateTimeFormat(locale, options).format(date);
  }
}
