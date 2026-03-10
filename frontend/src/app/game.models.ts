export interface CellCoordinate {
  x: number;
  y: number;
}

export interface GameState {
  width: number;
  height: number;
  generation: number;
  speed: number;
  isPlaying: boolean;
  aliveCells: CellCoordinate[];
  availablePatterns: string[];
}
