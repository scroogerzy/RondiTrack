enum WindowClass { compact, medium, expanded }

/// Breakpoints are based on available window width, never device identity.
WindowClass windowClassForWidth(double width) => switch (width) {
  < 600 => WindowClass.compact,
  < 840 => WindowClass.medium,
  _ => WindowClass.expanded,
};
