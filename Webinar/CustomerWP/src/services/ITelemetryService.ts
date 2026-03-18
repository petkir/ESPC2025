export type SaveButtonLocation = 'top' | 'bottom';

export interface ISaveTelemetry {
  button: SaveButtonLocation;
  success: boolean;
}

export interface ITelemetryService {
  trackPageView(name: string): void;
  trackAction(name: string, telemetry: ISaveTelemetry): void;
  trackException?(error: Error, context?: Record<string, any>): void;
}
