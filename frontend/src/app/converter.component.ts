import { Component, OnInit, computed, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService, ConvertRequest, ConvertResult, UnitDefinition } from './api.service';

type Theme = 'light' | 'dark';

@Component({
  selector: 'tmc-converter',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './converter.component.html',
  styleUrl: './converter.component.scss'
})
export class ConverterComponent implements OnInit {
  private readonly api = inject(ApiService);

  // TODO: Consider promoting some of these signals into a small view-model type
  // if the converter grows (multiple panels, history, etc.).
  readonly units = signal<UnitDefinition[]>([]);
  readonly loadingUnits = signal(false);
  readonly loadingConvert = signal(false);
  readonly error = signal<string | null>(null);

  fromUnit = signal<string>('cm');
  toUnit = signal<string>('m');
  value = signal<number>(1);

  readonly result = signal<ConvertResult | null>(null);
  
  readonly theme = signal<Theme>(this.getTheme());

  readonly metricUnits = computed(() =>
    this.units().filter(u => u.system === 'Metric')
  );

  readonly categories = computed(() =>
    Array.from(new Set(this.units().map(u => u.category)))
  );

  constructor() {
    // Log any errors to the console for easier debugging during development.
    effect(() => {
      if (this.error()) {
        // eslint-disable-next-line no-console
        console.error('Converter error:', this.error());
      }
    });
    
    // Watch for theme changes on root element
    const observer = new MutationObserver(() => {
      const newTheme = document.documentElement.getAttribute('data-theme') as Theme;
      if (newTheme && newTheme !== this.theme()) {
        this.theme.set(newTheme);
      }
    });
    observer.observe(document.documentElement, { attributes: true, attributeFilter: ['data-theme'] });
  }

  ngOnInit(): void {
    this.fetchUnits();
  }
  
  private getTheme(): Theme {
    const theme = document.documentElement.getAttribute('data-theme');
    return (theme === 'light' || theme === 'dark') ? theme : 'dark';
  }

  fetchUnits(): void {
    this.loadingUnits.set(true);
    this.error.set(null);

    this.api.getUnits().subscribe({
      next: units => {
        this.units.set(units);
        const hasCm = units.some(u => u.symbol === 'cm');
        const hasM = units.some(u => u.symbol === 'm');
        if (hasCm) {
          this.fromUnit.set('cm');
        }
        if (hasM) {
          this.toUnit.set('m');
        }
      },
      error: err => {
        this.error.set('Unable to load units from the API. Check that the backend is running on http://localhost:5080.');
        this.loadingUnits.set(false);
      },
      complete: () => this.loadingUnits.set(false)
    });
  }

  submit(): void {
    if (!this.value() && this.value() !== 0) {
      this.error.set('Please enter a numeric value to convert.');
      return;
    }

    this.loadingConvert.set(true);
    this.error.set(null);
    this.result.set(null);

    const body: ConvertRequest = {
      from: this.fromUnit(),
      to: this.toUnit(),
      value: this.value()
    };

    this.api.convert(body).subscribe({
      next: res => {
        if (!res.isOk && res.error) {
          this.error.set(res.error);
        }
        this.result.set(res);
      },
      error: () => {
        this.error.set('Conversion failed. Verify the API is reachable and try again.');
        this.loadingConvert.set(false);
      },
      complete: () => this.loadingConvert.set(false)
    });
  }

  trackBySymbol(_index: number, unit: UnitDefinition): string {
    return unit.symbol;
  }
}

