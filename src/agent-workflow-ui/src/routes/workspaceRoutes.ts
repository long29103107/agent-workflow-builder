export type WorkspaceRoute = "request" | "planner" | "kanban" | "configuration";

export type WorkspaceRouteDefinition = {
  id: WorkspaceRoute;
  label: string;
  path: string;
};

export const workspaceRoutes: WorkspaceRouteDefinition[] = [
  { id: "request", label: "Request", path: "/request" },
  { id: "planner", label: "Agent Planner", path: "/planner" },
  { id: "kanban", label: "Kanban Board", path: "/kanban" },
  { id: "configuration", label: "Repo & API Key", path: "/configuration" }
];

export function resolveWorkspaceRoute(pathname: string): WorkspaceRoute {
  const normalizedPath = pathname.toLowerCase();
  const match = workspaceRoutes.find((route) => route.path === normalizedPath);
  return match?.id ?? "request";
}

export function getWorkspaceRoutePath(routeId: WorkspaceRoute): string {
  return workspaceRoutes.find((route) => route.id === routeId)?.path ?? "/request";
}
