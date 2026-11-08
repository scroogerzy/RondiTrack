import { describe, expect, it } from 'vitest';
import { joinFlowFromUrl, joinStepNumber, transitionJoinFlow } from './joinFlow';

describe('join flow state machine', () => {
  it('restores the invite code and step from the URL', () => {
    const state = joinFlowFromUrl('invite-token', '3');
    expect(state).toEqual({ step: 'confirm', code: 'invite-token' });
  });
  it('moves forward and backward without losing the code', () => {
    const start = joinFlowFromUrl('invite-token', '1');
    const terms = transitionJoinFlow(start, 'continue');
    const confirm = transitionJoinFlow(terms, 'continue');
    expect(joinStepNumber(confirm)).toBe(3);
    expect(transitionJoinFlow(confirm, 'back')).toEqual(terms);
    expect(confirm.code).toBe('invite-token');
  });
  it('does not move beyond either end', () => {
    const start = joinFlowFromUrl('code', '1');
    const end = joinFlowFromUrl('code', '3');
    expect(transitionJoinFlow(start, 'back')).toEqual(start);
    expect(transitionJoinFlow(end, 'continue')).toEqual(end);
  });
});
