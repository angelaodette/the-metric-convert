import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: UserDto;
  expiresIn: number;
}

export interface UserDto {
  id: string;
  email: string;
  displayName?: string;
  avatarUrl?: string;
  emailVerified: boolean;
  roles: string[];
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
  deviceHint?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5080/api/auth';
  private currentUser = signal<UserDto | null>(null);
  private isAuthenticated = signal(false);
  private accessToken = signal<string | null>(null);
  private refreshToken = signal<string | null>(null);

  constructor(private http: HttpClient) {
    this.loadFromStorage();
  }

  // Public signals for template binding
  get user() {
    return this.currentUser;
  }

  get authenticated() {
    return this.isAuthenticated;
  }

  get token() {
    return this.accessToken;
  }

  register(email: string, password: string, displayName?: string): Observable<AuthResponse> {
    const request: RegisterRequest = { email, password, displayName };
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, request).pipe(
      tap(response => this.setAuthState(response))
    );
  }

  login(email: string, password: string, deviceHint?: string): Observable<AuthResponse> {
    const request: LoginRequest = { email, password, deviceHint };
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => this.setAuthState(response))
    );
  }

  refreshAccessToken(refreshToken: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh`, { refreshToken }).pipe(
      tap(response => this.setAuthState(response))
    );
  }

  logout(): void {
    this.currentUser.set(null);
    this.isAuthenticated.set(false);
    this.accessToken.set(null);
    this.refreshToken.set(null);
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('user');
  }

  private setAuthState(response: AuthResponse): void {
    this.currentUser.set(response.user);
    this.accessToken.set(response.accessToken);
    this.refreshToken.set(response.refreshToken);
    this.isAuthenticated.set(true);

    // Persist to localStorage
    localStorage.setItem('access_token', response.accessToken);
    localStorage.setItem('refresh_token', response.refreshToken);
    localStorage.setItem('user', JSON.stringify(response.user));
  }

  private loadFromStorage(): void {
    const token = localStorage.getItem('access_token');
    const user = localStorage.getItem('user');
    const refreshToken = localStorage.getItem('refresh_token');

    if (token && user) {
      this.accessToken.set(token);
      this.refreshToken.set(refreshToken);
      this.currentUser.set(JSON.parse(user));
      this.isAuthenticated.set(true);
    }
  }
}
