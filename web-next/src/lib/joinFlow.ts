export type JoinStep = 'code' | 'terms' | 'confirm';
export type JoinFlowState =
  | { step: 'code'; code: string }
  | { step: 'terms'; code: string }
  | { step: 'confirm'; code: string };
export type JoinFlowEvent = 'continue' | 'back';

export function joinFlowFromUrl(code: string, value?: string): JoinFlowState {
  const step: JoinStep = value === '2' ? 'terms' : value === '3' ? 'confirm' : 'code';
  return { step, code } as JoinFlowState;
}

export function transitionJoinFlow(state: JoinFlowState, event: JoinFlowEvent): JoinFlowState {
  const steps: JoinStep[] = ['code', 'terms', 'confirm'];
  const current = steps.indexOf(state.step);
  const next = Math.min(2, Math.max(0, current + (event === 'continue' ? 1 : -1)));
  return { step: steps[next], code: state.code } as JoinFlowState;
}

export function joinStepNumber(state: JoinFlowState): 1 | 2 | 3 {
  return state.step === 'code' ? 1 : state.step === 'terms' ? 2 : 3;
}
