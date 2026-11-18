enum StokvelRole {
  member('Member'),
  treasurer('Treasurer'),
  chairperson('Chairperson'),
  unknown('');

  new(this.apiValue);
  final String apiValue;

  static StokvelRole fromApi(String? value) => switch (value) {
    'Member' => member,
    'Treasurer' => treasurer,
    'Chairperson' => chairperson,
    _ => unknown,
  };
}

enum CycleStatus {
  scheduled,
  open,
  closed,
  payoutProcessed,
  unknown;

  static CycleStatus fromApi(String? value) => switch (value) {
    'Open' => open,
    'Closed' => closed,
    'PaidOut' => payoutProcessed,
    _ => unknown,
  };

  String get label => switch (this) {
    scheduled => 'Scheduled',
    open => 'Open',
    closed => 'Closed',
    payoutProcessed => 'Paid out',
    unknown => 'Unknown status',
  };
}

enum ContributionStatus {
  recorded,
  pendingProof,
  confirmed,
  unknown;

  // The supplied API ContributionResponse has no status field. These are
  // local UI states only; never deserialize them as API values.
  static ContributionStatus fromApi(String? _) => unknown;

  String get label => switch (this) {
    recorded => 'Recorded',
    pendingProof => 'Proof pending',
    confirmed => 'Confirmed',
    unknown => 'Unknown status',
  };
}

enum StokvelAction { pay, uploadProof, confirm, closeCycle, recordPayout }

CycleStatus deriveCycleStatus({
  required DateTime nowUtc,
  required DateTime startUtc,
  required DateTime endUtc,
  required DateTime? closedAtUtc,
  required DateTime? payoutProcessedAtUtc,
}) {
  if (payoutProcessedAtUtc != null) return CycleStatus.payoutProcessed;
  if (closedAtUtc != null) return CycleStatus.closed;
  if (nowUtc.isBefore(startUtc)) return CycleStatus.scheduled;
  if (!nowUtc.isAfter(endUtc)) return CycleStatus.open;
  return CycleStatus.unknown;
}

Set<StokvelAction> availableActions({
  required StokvelRole role,
  required CycleStatus cycleStatus,
  required ContributionStatus contributionStatus,
  required bool hasOwnContribution,
}) => switch (role) {
  StokvelRole.unknown => const {},
  StokvelRole.member => switch (cycleStatus) {
    CycleStatus.scheduled ||
    CycleStatus.closed ||
    CycleStatus.payoutProcessed ||
    CycleStatus.unknown => const {},
    CycleStatus.open => switch (contributionStatus) {
      ContributionStatus.unknown =>
        hasOwnContribution
            ? const {}
            : {StokvelAction.pay, StokvelAction.uploadProof},
      ContributionStatus.recorded =>
        hasOwnContribution
            ? {StokvelAction.uploadProof}
            : {StokvelAction.pay, StokvelAction.uploadProof},
      ContributionStatus.pendingProof ||
      ContributionStatus.confirmed => const {},
    },
  },
  StokvelRole.treasurer || StokvelRole.chairperson => switch (cycleStatus) {
    CycleStatus.scheduled ||
    CycleStatus.payoutProcessed ||
    CycleStatus.unknown => const {},
    CycleStatus.open => {StokvelAction.confirm, StokvelAction.closeCycle},
    CycleStatus.closed => {StokvelAction.recordPayout},
  },
};
