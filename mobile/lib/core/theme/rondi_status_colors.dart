import 'package:flutter/material.dart';

@immutable
class RondiStatusColors extends ThemeExtension<RondiStatusColors> {
  const new({
    required this.openForeground,
    required this.openBackground,
    required this.closedForeground,
    required this.closedBackground,
    required this.paidOutForeground,
    required this.paidOutBackground,
    required this.arrearsForeground,
    required this.arrearsBackground,
    required this.contributionPaidForeground,
    required this.contributionPaidBackground,
    required this.contributionPendingForeground,
    required this.contributionPendingBackground,
  });

  final Color openForeground;
  final Color openBackground;
  final Color closedForeground;
  final Color closedBackground;
  final Color paidOutForeground;
  final Color paidOutBackground;
  final Color arrearsForeground;
  final Color arrearsBackground;
  final Color contributionPaidForeground;
  final Color contributionPaidBackground;
  final Color contributionPendingForeground;
  final Color contributionPendingBackground;

  static const light = RondiStatusColors(
    openForeground: Color(0xFF0B5D45),
    openBackground: Color(0xFFE1F4EC),
    closedForeground: Color(0xFF424242),
    closedBackground: Color(0xFFEEEEEE),
    paidOutForeground: Color(0xFF174EA6),
    paidOutBackground: Color(0xFFE8F0FE),
    arrearsForeground: Color(0xFF8A1C1C),
    arrearsBackground: Color(0xFFFCE8E6),
    contributionPaidForeground: Color(0xFF0B5D45),
    contributionPaidBackground: Color(0xFFE1F4EC),
    contributionPendingForeground: Color(0xFF704500),
    contributionPendingBackground: Color(0xFFFFF4D6),
  );

  static const dark = RondiStatusColors(
    openForeground: Color(0xFFA6E9CD),
    openBackground: Color(0xFF173B31),
    closedForeground: Color(0xFFE0E0E0),
    closedBackground: Color(0xFF3A3A3A),
    paidOutForeground: Color(0xFFB7D1FF),
    paidOutBackground: Color(0xFF1C3558),
    arrearsForeground: Color(0xFFFFB4AB),
    arrearsBackground: Color(0xFF5A1B1B),
    contributionPaidForeground: Color(0xFFA6E9CD),
    contributionPaidBackground: Color(0xFF173B31),
    contributionPendingForeground: Color(0xFFFFD58A),
    contributionPendingBackground: Color(0xFF503700),
  );

  @override
  RondiStatusColors copyWith({
    Color? openForeground,
    Color? openBackground,
    Color? closedForeground,
    Color? closedBackground,
    Color? paidOutForeground,
    Color? paidOutBackground,
    Color? arrearsForeground,
    Color? arrearsBackground,
    Color? contributionPaidForeground,
    Color? contributionPaidBackground,
    Color? contributionPendingForeground,
    Color? contributionPendingBackground,
  }) => RondiStatusColors(
    openForeground: openForeground ?? this.openForeground,
    openBackground: openBackground ?? this.openBackground,
    closedForeground: closedForeground ?? this.closedForeground,
    closedBackground: closedBackground ?? this.closedBackground,
    paidOutForeground: paidOutForeground ?? this.paidOutForeground,
    paidOutBackground: paidOutBackground ?? this.paidOutBackground,
    arrearsForeground: arrearsForeground ?? this.arrearsForeground,
    arrearsBackground: arrearsBackground ?? this.arrearsBackground,
    contributionPaidForeground:
        contributionPaidForeground ?? this.contributionPaidForeground,
    contributionPaidBackground:
        contributionPaidBackground ?? this.contributionPaidBackground,
    contributionPendingForeground:
        contributionPendingForeground ?? this.contributionPendingForeground,
    contributionPendingBackground:
        contributionPendingBackground ?? this.contributionPendingBackground,
  );

  @override
  RondiStatusColors lerp(ThemeExtension<RondiStatusColors>? other, double t) {
    if (other is! RondiStatusColors) return this;
    return RondiStatusColors(
      openForeground: Color.lerp(openForeground, other.openForeground, t)!,
      openBackground: Color.lerp(openBackground, other.openBackground, t)!,
      closedForeground: Color.lerp(
        closedForeground,
        other.closedForeground,
        t,
      )!,
      closedBackground: Color.lerp(
        closedBackground,
        other.closedBackground,
        t,
      )!,
      paidOutForeground: Color.lerp(
        paidOutForeground,
        other.paidOutForeground,
        t,
      )!,
      paidOutBackground: Color.lerp(
        paidOutBackground,
        other.paidOutBackground,
        t,
      )!,
      arrearsForeground: Color.lerp(
        arrearsForeground,
        other.arrearsForeground,
        t,
      )!,
      arrearsBackground: Color.lerp(
        arrearsBackground,
        other.arrearsBackground,
        t,
      )!,
      contributionPaidForeground: Color.lerp(
        contributionPaidForeground,
        other.contributionPaidForeground,
        t,
      )!,
      contributionPaidBackground: Color.lerp(
        contributionPaidBackground,
        other.contributionPaidBackground,
        t,
      )!,
      contributionPendingForeground: Color.lerp(
        contributionPendingForeground,
        other.contributionPendingForeground,
        t,
      )!,
      contributionPendingBackground: Color.lerp(
        contributionPendingBackground,
        other.contributionPendingBackground,
        t,
      )!,
    );
  }
}
