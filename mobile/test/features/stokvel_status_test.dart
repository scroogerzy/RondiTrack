import 'package:flutter_test/flutter_test.dart';
import 'package:ronditrack_mobile/features/stokvel/domain/stokvel_status.dart';

void main() {
  test('maps exact API role and cycle strings without throwing', () {
    expect(StokvelRole.fromApi('Treasurer'), StokvelRole.treasurer);
    expect(StokvelRole.fromApi('treasurer'), StokvelRole.unknown);
    expect(CycleStatus.fromApi('Open'), CycleStatus.open);
    expect(CycleStatus.fromApi('PaidOut'), CycleStatus.payoutProcessed);
    expect(CycleStatus.fromApi('paidout'), CycleStatus.unknown);
    expect(ContributionStatus.fromApi('Recorded'), ContributionStatus.unknown);
  });

  test('only a manager gets cycle-management actions in the open cycle', () {
    final member = availableActions(
      role: StokvelRole.member,
      cycleStatus: CycleStatus.open,
      contributionStatus: ContributionStatus.unknown,
      hasOwnContribution: false,
    );
    expect(member, contains(StokvelAction.pay));
    expect(member, isNot(contains(StokvelAction.closeCycle)));
    final treasurer = availableActions(
      role: StokvelRole.treasurer,
      cycleStatus: CycleStatus.open,
      contributionStatus: ContributionStatus.unknown,
      hasOwnContribution: false,
    );
    expect(
      treasurer,
      containsAll([StokvelAction.confirm, StokvelAction.closeCycle]),
    );
    expect(treasurer, isNot(contains(StokvelAction.pay)));
  });

  test('unknown role or status grants no actions', () {
    expect(
      availableActions(
        role: StokvelRole.unknown,
        cycleStatus: CycleStatus.open,
        contributionStatus: ContributionStatus.unknown,
        hasOwnContribution: false,
      ),
      isEmpty,
    );
    expect(
      availableActions(
        role: StokvelRole.treasurer,
        cycleStatus: CycleStatus.unknown,
        contributionStatus: ContributionStatus.unknown,
        hasOwnContribution: false,
      ),
      isEmpty,
    );
  });
  test('only the Treasurer can record a closed-cycle payout', () {
    final treasurer = availableActions(
      role: StokvelRole.treasurer,
      cycleStatus: CycleStatus.closed,
      contributionStatus: ContributionStatus.unknown,
      hasOwnContribution: false,
    );
    final chairperson = availableActions(
      role: StokvelRole.chairperson,
      cycleStatus: CycleStatus.closed,
      contributionStatus: ContributionStatus.unknown,
      hasOwnContribution: false,
    );
    expect(treasurer, contains(StokvelAction.recordPayout));
    expect(chairperson, isNot(contains(StokvelAction.recordPayout)));
  });
}

// Assignment 10.3: payout recording is a Treasurer-only membership action.
