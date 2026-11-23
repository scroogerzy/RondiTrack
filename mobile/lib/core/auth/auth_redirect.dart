/// Keeps invite redirects inside the application.
String? authRedirect({
  required bool isSignedIn,
  required String location,
  String? rememberedLocation,
}) {
  final path = Uri.tryParse(location)?.path ?? location;

  if (!isSignedIn) {
    if (path == '/sign-in') return null;
    final invite = _safeInvitePath(path);
    if (invite != null) {
      return Uri(path: '/sign-in', queryParameters: {'from': invite}).toString();
    }
    return '/sign-in';
  }

  if (path == '/sign-in' || path == '/') {
    return _safeInvitePath(rememberedLocation) ?? '/home';
  }
  return null;
}

String? _safeInvitePath(String? value) {
  if (value == null || value.isEmpty) return null;
  final uri = Uri.tryParse(value);
  if (uri == null || uri.hasScheme || uri.hasAuthority ||
      !uri.path.startsWith('/join/')) return null;
  if (uri.pathSegments.length != 2 || uri.pathSegments.last.isEmpty) {
    return null;
  }
  return uri.path;
}
