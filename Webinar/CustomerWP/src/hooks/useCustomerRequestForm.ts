import * as React from 'react';

import type { ISector } from '../models/Sector';
import type { ICustomerRequest, NdaDecision } from '../models/CustomerRequest';
import { useServices } from '../contexts/ServiceContext';
import type { SaveButtonLocation } from '../services/ITelemetryService';

export interface ICustomerRequestFormState {
  title: string;
  sectorId: number | undefined;
  ndaDecision: NdaDecision;
}

export interface ICustomerRequestFormErrors {
  title?: string;
  sectorId?: string;
  ndaDecision?: string;
}

function getDurationBucket(durationMs: number): string {
  if (durationMs < 100) return '<100ms';
  if (durationMs < 500) return '100-499ms';
  if (durationMs < 1000) return '500-999ms';
  if (durationMs < 3000) return '1-2.9s';
  return '>=3s';
}

export function useCustomerRequestForm(): {
  sectors: ISector[];
  sectorsLoading: boolean;
  form: ICustomerRequestFormState;
  selectedSector: ISector | undefined;
  errors: ICustomerRequestFormErrors;
  isSaving: boolean;
  lastSavedId: number | undefined;
  saveError: string | undefined;
  setTitle: (value: string) => void;
  setSectorId: (value: number | undefined) => void;
  setNdaDecision: (value: NdaDecision) => void;
  save: (button: SaveButtonLocation) => Promise<void>;
} {
  const { dataService, telemetryService } = useServices();

  const [sectors, setSectors] = React.useState<ISector[]>([]);
  const [sectorsLoading, setSectorsLoading] = React.useState<boolean>(true);

  const [title, setTitle] = React.useState<string>('');
  const [sectorId, setSectorIdState] = React.useState<number | undefined>(undefined);
  const [ndaDecision, setNdaDecisionState] = React.useState<NdaDecision>(undefined);

  const [errors, setErrors] = React.useState<ICustomerRequestFormErrors>({});
  const [isSaving, setIsSaving] = React.useState<boolean>(false);
  const [lastSavedId, setLastSavedId] = React.useState<number | undefined>(undefined);
  const [saveError, setSaveError] = React.useState<string | undefined>(undefined);

  React.useEffect(() => {
    telemetryService.trackPageView('CustomerForm');

    let cancelled = false;

    dataService
      .getSectors()
      .then(result => {
        if (!cancelled) setSectors(result);
      })
      .catch(() => {
        // Swallow errors here; the form can still render with an empty sector list
      })
      .then(() => {
        if (!cancelled) setSectorsLoading(false);
      }).catch(() => {
        // none
      });

    return () => {
      cancelled = true;
    };
  }, [dataService, telemetryService]);

  const selectedSector = React.useMemo(
    () => sectors.find(s => s.id === sectorId),
    [sectors, sectorId]
  );

  const setSectorId = React.useCallback(
    (value: number | undefined) => {
      setSectorIdState(value);
      setErrors(prev => ({ ...prev, sectorId: undefined }));
      // If the new sector doesn't require NDA, we can clear the selection
      const sector = sectors.find(s => s.id === value);
      if (!sector?.requiresNda) {
        setNdaDecisionState(undefined);
        setErrors(prev => ({ ...prev, ndaDecision: undefined }));
      }
    },
    [sectors]
  );

  const setNdaDecision = React.useCallback((value: NdaDecision) => {
    setNdaDecisionState(value);
    setErrors(prev => ({ ...prev, ndaDecision: undefined }));
  }, []);

  const validate = React.useCallback((): ICustomerRequestFormErrors => {
    const next: ICustomerRequestFormErrors = {};

    if (!title.trim()) {
      next.title = 'Title is required.';
    }

    if (!sectorId) {
      next.sectorId = 'Sector is required.';
    }

    if (selectedSector?.requiresNda && !ndaDecision) {
      next.ndaDecision = 'NDA decision is required for this sector.';
    }

    return next;
  }, [title, sectorId, selectedSector, ndaDecision]);

  const save = React.useCallback(
    async (button: SaveButtonLocation) => {
      setSaveError(undefined);
      setLastSavedId(undefined);

      const nextErrors = validate();
      setErrors(nextErrors);
      if (Object.keys(nextErrors).length > 0) {
        return;
      }

      if (!selectedSector) {
        return;
      }

      const request: ICustomerRequest = {
        title: title.trim(),
        sectorId: selectedSector.id,
        sectorTitle: selectedSector.title,
        sectorRequiresNda: selectedSector.requiresNda,
        ndaDecision
      };

      setIsSaving(true);
      try {
        const result = await dataService.saveRequest(request);
       
        telemetryService.trackAction('Save', {
          button,
          success: true
        });

        setLastSavedId(result.id);
      } catch (e) {

        telemetryService.trackAction('Save', {
          button,
          success: false
    });

        setSaveError(e instanceof Error ? e.message : 'Save failed.');
      } finally {
        setIsSaving(false);
      }
    },
    [dataService, ndaDecision, selectedSector, telemetryService, title, validate]
  );

  return {
    sectors,
    sectorsLoading,
    form: { title, sectorId, ndaDecision },
    selectedSector,
    errors,
    isSaving,
    lastSavedId,
    saveError,
    setTitle: (value: string) => {
      setTitle(value);
      setErrors(prev => ({ ...prev, title: undefined }));
    },
    setSectorId,
    setNdaDecision,
    save
  };
}
