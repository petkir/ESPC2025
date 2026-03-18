import * as React from 'react';

import { ServiceProvider } from '../../../contexts/ServiceContext';
import { CustomerRequestForm } from '../../../components/CustomerRequestForm/CustomerRequestForm';
import type { ICustomerFormProps } from './ICustomerFormProps';

const CustomerForm: React.FC<ICustomerFormProps> = props => {
  const { dataService, telemetryService, hasTeamsContext } = props;

  return (
    <ServiceProvider dataService={dataService} telemetryService={telemetryService}>
      <CustomerRequestForm hasTeamsContext={hasTeamsContext} />
    </ServiceProvider>
  );
};

export default CustomerForm;
