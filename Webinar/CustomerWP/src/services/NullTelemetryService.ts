import type { ITelemetryService, ISaveTelemetry } from './ITelemetryService';

export class NullTelemetryService implements ITelemetryService {
  public trackPageView(_name: string): void {
    // no-op
  }

  public trackAction(_name: string, _telemetry: ISaveTelemetry): void {
    // no-op
  }
  public trackException?(_error: Error, _context?: Record<string, any>): void {
    // no-op
  }
}
