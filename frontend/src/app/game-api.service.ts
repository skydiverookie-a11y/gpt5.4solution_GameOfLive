import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { GameState } from './game.models';

@Injectable({ providedIn: 'root' })
export class GameApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/game';

  getState(): Observable<GameState> {
    return this.http.get<GameState>(this.baseUrl);
  }

  configureGrid(width: number, height: number): Observable<GameState> {
    return this.http.put<GameState>(`${this.baseUrl}/grid`, { width, height });
  }

  toggleCell(x: number, y: number): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/cells/toggle`, { x, y });
  }

  clear(): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/clear`, {});
  }

  randomize(density: number): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/randomize`, { density });
  }

  loadPattern(name: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/patterns/${encodeURIComponent(name)}`, {});
  }

  step(): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/step`, {});
  }

  reset(): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/reset`, {});
  }

  setPlayback(isPlaying: boolean): Observable<GameState> {
    return this.http.put<GameState>(`${this.baseUrl}/playback`, { isPlaying });
  }

  setSpeed(speed: number): Observable<GameState> {
    return this.http.put<GameState>(`${this.baseUrl}/speed`, { speed });
  }
}
