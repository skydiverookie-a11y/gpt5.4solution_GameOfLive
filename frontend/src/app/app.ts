import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { GameApiService } from './game-api.service';
import { GameState } from './game.models';

@Component({
  selector: 'app-root',
  imports: [FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly api = inject(GameApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly state = signal<GameState | null>(null);
  readonly error = signal('');
  readonly sizeWidth = signal(30);
  readonly sizeHeight = signal(30);
  readonly density = signal(35);
  readonly selectedPattern = signal('Glider');
  readonly aliveCellKeys = computed(() => {
    const snapshot = this.state();
    return new Set(snapshot?.aliveCells.map((cell) => `${cell.x}:${cell.y}`) ?? []);
  });
  readonly rows = computed(() => Array.from({ length: this.state()?.height ?? 0 }, (_, value) => value));
  readonly columns = computed(() => Array.from({ length: this.state()?.width ?? 0 }, (_, value) => value));

  private pollHandle: number | null = null;

  constructor() {
    this.fetchState();
    this.destroyRef.onDestroy(() => this.stopPolling());
  }

  isAlive(x: number, y: number): boolean {
    return this.aliveCellKeys().has(`${x}:${y}`);
  }

  applyGridSize(): void {
    this.runRequest(this.api.configureGrid(this.sizeWidth(), this.sizeHeight()));
  }

  setWidth(value: number | string): void {
    this.sizeWidth.set(Number(value));
  }

  setHeight(value: number | string): void {
    this.sizeHeight.set(Number(value));
  }

  setDensity(value: number | string): void {
    this.density.set(Number(value));
  }

  setPattern(value: string): void {
    this.selectedPattern.set(value);
  }

  toggleCell(x: number, y: number): void {
    this.runRequest(this.api.toggleCell(x, y));
  }

  clearGrid(): void {
    this.runRequest(this.api.clear());
  }

  randomizeGrid(): void {
    this.runRequest(this.api.randomize(this.density()));
  }

  loadPattern(): void {
    this.runRequest(this.api.loadPattern(this.selectedPattern()));
  }

  step(): void {
    this.runRequest(this.api.step());
  }

  reset(): void {
    this.runRequest(this.api.reset());
  }

  togglePlayback(): void {
    const nextValue = !this.state()?.isPlaying;
    this.runRequest(this.api.setPlayback(nextValue));
  }

  updateSpeed(speed: number): void {
    this.runRequest(this.api.setSpeed(Number(speed)), false);
  }

  trackByValue(_: number, value: number): number {
    return value;
  }

  private fetchState(): void {
    this.runRequest(this.api.getState(), false);
  }

  private runRequest(request: ReturnType<GameApiService['getState']>, keepError = true): void {
    if (!keepError) {
      this.error.set('');
    }

    request.subscribe({
      next: (state) => {
        this.state.set(state);
        this.sizeWidth.set(state.width);
        this.sizeHeight.set(state.height);
        if (!state.availablePatterns.includes(this.selectedPattern())) {
          this.selectedPattern.set(state.availablePatterns[0] ?? 'Glider');
        }
        this.error.set('');
        this.syncPolling(state.isPlaying);
      },
      error: (error: HttpErrorResponse) => {
        const message = typeof error.error?.error === 'string' ? error.error.error : 'Request failed.';
        this.error.set(message);
        this.syncPolling(false);
      }
    });
  }

  private syncPolling(isPlaying: boolean): void {
    if (isPlaying) {
      this.startPolling();
      return;
    }

    this.stopPolling();
  }

  private startPolling(): void {
    if (this.pollHandle !== null) {
      return;
    }

    this.pollHandle = window.setInterval(() => this.fetchState(), 150);
  }

  private stopPolling(): void {
    if (this.pollHandle === null) {
      return;
    }

    window.clearInterval(this.pollHandle);
    this.pollHandle = null;
  }
}
