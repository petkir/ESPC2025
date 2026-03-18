import type { WebPartContext } from '@microsoft/sp-webpart-base';
import { spfi, SPFI } from '@pnp/sp';
import { SPFx } from '@pnp/sp/behaviors/spfx';
import '@pnp/sp/webs';
import '@pnp/sp/lists';
import '@pnp/sp/fields';
import '@pnp/sp/items';

import type { ISector } from '../models/Sector';
import type { ICustomerRequest } from '../models/CustomerRequest';
import type { ICustomerFormDataService, ISaveResult } from './ICustomerFormDataService';

export interface ISpCustomerFormDataServiceOptions {
  sectorsListTitle: string;
  requestsListTitle: string;
}

export class SpCustomerFormDataService implements ICustomerFormDataService {
  private readonly _sp: SPFI;
  private readonly _sectorsListTitle: string;
  private readonly _requestsListTitle: string;

  public constructor(context: WebPartContext, options: ISpCustomerFormDataServiceOptions) {
    this._sp = spfi().using(SPFx(context));
    this._sectorsListTitle = options.sectorsListTitle;
    this._requestsListTitle = options.requestsListTitle;
  }

  public async getSectors(): Promise<ISector[]> {
    const items = await this._sp.web.lists
      .getByTitle(this._sectorsListTitle)
      .items.select('Id', 'Title', 'RequiresNda')
      .top(500)();

    return items.map(i => ({
      id: i.Id as number,
      title: (i.Title as string) ?? '',
      requiresNda: Boolean((i as { RequiresNda?: boolean }).RequiresNda)
    }));
  }

  public async saveRequest(request: ICustomerRequest): Promise<ISaveResult> {
    await this._ensureRequestsList();

    const addResult = await this._sp.web.lists
      .getByTitle(this._requestsListTitle)
      .items.add({
        Title: request.title,
        Sector: request.sectorId,
        SectorRequiresNda: request.sectorRequiresNda,
        NdaDecision: request.ndaDecision ?? null
      });

    return { id: addResult.data.Id as number };
  }

  private async _ensureRequestsList(): Promise<void> {
    const list = this._sp.web.lists.getByTitle(this._requestsListTitle);

    try {
      await list.select('Id')();
      return;
    } catch {
      // Create the list if missing
    }

    await this._sp.web.lists.add(this._requestsListTitle, 'Customer requests', 100, false);

    const createdList = this._sp.web.lists.getByTitle(this._requestsListTitle);

    await this._ensureField(createdList, 'SectorId', async () => createdList.fields.addNumber('SectorId'));
    await this._ensureField(createdList, 'SectorTitle', async () => createdList.fields.addText('SectorTitle'));
    await this._ensureField(createdList, 'SectorRequiresNda', async () => createdList.fields.addBoolean('SectorRequiresNda'));
    await this._ensureField(createdList, 'NdaDecision', async () => createdList.fields.addText('NdaDecision'));
  }

  private async _ensureField(
    list: ReturnType<SPFI['web']['lists']['getByTitle']>,
    internalName: string,
    create: () => Promise<unknown>
  ): Promise<void> {
    try {
      await list.fields.getByInternalNameOrTitle(internalName).select('Id')();
    } catch {
      try {
        await create();
      } catch {
        // Ignore field creation errors (e.g., if another client created it)
      }
    }
  }
}
