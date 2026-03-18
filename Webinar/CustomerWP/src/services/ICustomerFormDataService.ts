import type { ISector } from '../models/Sector';
import type { ICustomerRequest } from '../models/CustomerRequest';

export interface ISaveResult {
  id: number;
}

export interface ICustomerFormDataService {
  getSectors(): Promise<ISector[]>;
  saveRequest(request: ICustomerRequest): Promise<ISaveResult>;
}
