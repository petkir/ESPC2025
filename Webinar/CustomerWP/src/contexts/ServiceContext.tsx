import * as React from 'react';

import type { ICustomerFormDataService } from '../services/ICustomerFormDataService';
import type { ITelemetryService } from '../services/ITelemetryService';

export interface IServiceContextValue {
  dataService: ICustomerFormDataService;
  telemetryService: ITelemetryService;
}

const ServiceContext = React.createContext<IServiceContextValue | undefined>(undefined);

export const ServiceProvider: React.FC<React.PropsWithChildren<IServiceContextValue>> = props => {
  const { dataService, telemetryService, children } = props;

  return (
    <ServiceContext.Provider value={{ dataService, telemetryService }}>
      {children}
    </ServiceContext.Provider>
  );
};

export function useServices(): IServiceContextValue {
  const ctx = React.useContext(ServiceContext);
  if (!ctx) {
    throw new Error('useServices must be used within a ServiceProvider');
  }

  return ctx;
}
