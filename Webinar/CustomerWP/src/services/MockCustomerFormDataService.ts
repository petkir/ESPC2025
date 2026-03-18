import type { ISector } from '../models/Sector';
import type { ICustomerRequest } from '../models/CustomerRequest';
import type { ICustomerFormDataService, ISaveResult } from './ICustomerFormDataService';

export class MockCustomerFormDataService implements ICustomerFormDataService {
  private readonly _sectors: ISector[] = [
    { id: 1, title: 'Energy', requiresNda: false },
    { id: 2, title: 'Healthcare', requiresNda: true },
    { id: 3, title: 'Finance', requiresNda: true }
  ];

  public async getSectors(): Promise<ISector[]> {
    return this._sectors;
  }

  public async saveRequest(request: ICustomerRequest): Promise<ISaveResult> {
    // Simulate latency
    await new Promise(resolve => setTimeout(resolve, 250));

    // eslint-disable-next-line no-console
    console.log('[MockCustomerFormDataService] saveRequest', request);

    return { id: Date.now() };
  }
}
