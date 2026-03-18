import type { ICustomerFormDataService } from '../../../services/ICustomerFormDataService';
import type { ITelemetryService } from '../../../services/ITelemetryService';

export interface ICustomerFormProps {
  hasTeamsContext: boolean;
  dataService: ICustomerFormDataService;
  telemetryService: ITelemetryService;
}
