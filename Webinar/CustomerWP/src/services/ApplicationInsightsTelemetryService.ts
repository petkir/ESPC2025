import { ApplicationInsights } from '@microsoft/applicationinsights-web';

import type { ISaveTelemetry, ITelemetryService } from './ITelemetryService';

export interface IApplicationInsightsTelemetryServiceOptions {
  connectionString: string;
}

export class ApplicationInsightsTelemetryService implements ITelemetryService {
  private readonly _appInsights: ApplicationInsights;

  public constructor(options: IApplicationInsightsTelemetryServiceOptions) {
    this._appInsights = new ApplicationInsights({
      config: {
        connectionString: options.connectionString
      }
    });

    this._appInsights.loadAppInsights();
  }

  public trackPageView(name: string): void {
    this._appInsights.trackPageView({ name });
  }

  public trackException(error: Error, context?: Record<string, any>): void {
    this._appInsights.trackException({ exception: error, properties: context });
  }

  public trackAction(name: string, telemetry: ISaveTelemetry): void {
    this._appInsights.trackEvent(
      {
        name: `CustomerForm.Action.${name}`,
      },
      {
        button: telemetry.button,
        success: telemetry.success
      }
    );
  }
}
