import * as React from 'react';
import {
  ChoiceGroup,
  type IChoiceGroupOption,
  Dropdown,
  type IDropdownOption,
  MessageBar,
  MessageBarType,
  PrimaryButton,
  Spinner,
  SpinnerSize,
  Stack,
  TextField
} from '@fluentui/react';

import styles from './CustomerRequestForm.module.scss';
import { useCustomerRequestForm } from '../../hooks/useCustomerRequestForm';
import type { NdaDecision } from '../../models/CustomerRequest';


export interface ICustomerRequestFormProps {
  hasTeamsContext: boolean;
}

export const CustomerRequestForm: React.FC<ICustomerRequestFormProps> = props => {
  const { hasTeamsContext } = props;

  const {
    sectors,
    sectorsLoading,
    form,
    selectedSector,
    errors,
    isSaving,
    lastSavedId,
    saveError,
    setTitle,
    setSectorId,
    setNdaDecision,
    save
  } = useCustomerRequestForm();
  
  // this would be the trackpageimplementation but we shifted it into the hook
  /*
  const { dataService, telemetryService } = useServices();
  React.useEffect(() => {
    telemetryService.trackPageView('CustomerRequestForm');
  }, [telemetryService]);
  */

  const sectorOptions: IDropdownOption[] = React.useMemo(
    () => sectors.map(s => ({ key: s.id, text: s.title })),
    [sectors]
  );

  const ndaOptions: IChoiceGroupOption[] = React.useMemo(
    () => [
      { key: 'Yes', text: 'Yes' },
      { key: 'No', text: 'No' }
    ],
    []
  );

  const ndaRequired = Boolean(selectedSector?.requiresNda);

  const onSaveTop = React.useCallback(() => {
    save('top').catch(() => undefined);
  }, [save]);

  const onSaveBottom = React.useCallback(() => {
    save('bottom').catch(() => undefined);
  }, [save]);

  return (
    <section className={`${styles.container} ${hasTeamsContext ? styles.teams : ''}`}>
      <Stack tokens={{ childrenGap: 12 }}>
        <div className={styles.actions}>
          <Stack horizontal horizontalAlign="end">
           <PrimaryButton text={isSaving ? 'Saving…' : 'Save'} onClick={onSaveTop} disabled={isSaving || sectorsLoading} />
          </Stack>
        </div>

        {sectorsLoading && (
          <Spinner label="Loading sectors" size={SpinnerSize.medium} />
        )}

        {Boolean(lastSavedId) && (
          <MessageBar className={styles.success} messageBarType={MessageBarType.success} isMultiline={false}>
            Saved (Item ID: {lastSavedId})
          </MessageBar>
        )}

        {saveError && (
          <MessageBar className={styles.error} messageBarType={MessageBarType.error} isMultiline={false}>
            {saveError}
          </MessageBar>
        )}

        <div className={styles.fieldRow}>
          <TextField
            label="Title"
            required
            value={form.title}
            onChange={(_, v) => setTitle(v ?? '')}
            errorMessage={errors.title}
          />
        </div>

        <div className={styles.fieldRow}>
          <Dropdown
            label="Sector"
            required
            options={sectorOptions}
            selectedKey={form.sectorId}
            onChange={(_, option) => setSectorId(option ? (option.key as number) : undefined)}
            errorMessage={errors.sectorId}
            disabled={sectorsLoading}
          />
        </div>

        <div className={styles.fieldRow}>
          <ChoiceGroup
            label="NDA"
            required={ndaRequired}
            options={ndaOptions}
            selectedKey={form.ndaDecision}
            onChange={(_, option) => setNdaDecision((option?.key as NdaDecision) ?? undefined)}
          />
          {errors.ndaDecision && (
            <div role="alert" aria-live="assertive" className={styles.errorText}>
              {errors.ndaDecision}
            </div>
          )}
        </div>

        <div className={styles.actions}>
          <Stack horizontal horizontalAlign="end">
            <PrimaryButton text={isSaving ? 'Saving…' : 'Save'} onClick={onSaveBottom} disabled={isSaving || sectorsLoading} />
          </Stack>
        </div>
      </Stack>
    </section>
  );
};
