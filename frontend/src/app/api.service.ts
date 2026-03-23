import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// Shape of the unit data coming from the backend.
// Kept aligned with TheMetricConvert.Api.UnitDefinition.
export interface UnitDefinition {
  symbol: string;
  name: string;
  category: string;
  system: string;
  factorToBase: number;
  baseSymbol: string;
  powerOfTen?: number | null;
}

// Request shape for POST /api/conversions.
export interface ConvertRequest {
  from: string;
  to: string;
  value: number;
}

// Response + teaching metadata returned by the conversions endpoint.
export interface ConvertResult {
  isOk: boolean;
  error?: string | null;
  input?: ConvertRequest | null;
  outputValue?: number | null;
  outputUnit?: string | null;
  steps: string[];
  tip?: string | null;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);

  // For now we assume both apps run locally.
  // TODO: Make this configurable via environment file for deploys.
  private readonly baseUrl = signal('http://localhost:5080');

  getUnits(): Observable<UnitDefinition[]> {
    return this.http.get<UnitDefinition[]>(`${this.baseUrl()}/api/units`);
  }

  convert(body: ConvertRequest): Observable<ConvertResult> {
    return this.http.post<ConvertResult>(`${this.baseUrl()}/api/conversions`, body);
  }
}

