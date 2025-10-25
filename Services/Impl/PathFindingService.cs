using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class PathfindingService : IPathfindingService
    {
        // Movimientos posibles (8 direcciones)
        private readonly (int dx, int dy)[] _directions =
        [
        (0, 1), (0, -1), (1, 0), (-1, 0), // Cardinales (Coste 10)
        (1, 1), (1, -1), (-1, 1), (-1, -1) // Diagonales (Coste 14)
        ];

        public List<(int X, int Y)> FindPath(
            Arena arena,
            TroopEntity troop,
            int startX, int startY,
            int targetX, int targetY)
        {
            // Conjunto de nodos a evaluar (Open Set) - Idealmente una Priority Queue para eficiencia.
            // Aquí se usa una List, por ser más simple en C#, que se ordena en cada iteración.
            var openSet = new List<PathfindingNode>();

            // Conjunto de nodos ya evaluados (Closed Set)
            var closedSet = new HashSet<PathfindingNode>();

            // Diccionario para acceder a los nodos de la cuadrícula
            var allNodes = new Dictionary<(int, int), PathfindingNode>();

            var startNode = GetNode(startX, startY, allNodes);
            var targetNode = GetNode(targetX, targetY, allNodes);

            startNode.GCost = 0;
            startNode.HCost = CalculateHCost(startX, startY, targetX, targetY);
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                // 1. Obtener el nodo con el menor FCost del Open Set
                var currentNode = openSet.MinBy(node => node.FCost)
                    ?? throw new InvalidOperationException("Open set empty unexpectedly.");

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                // 2. Comprobar si es el destino
                // Si el nodo actual está adyacente al destino, la ruta ha terminado.
                if (IsAdjacent(currentNode.X, currentNode.Y, targetX, targetY))
                {
                    return RetracePath(startNode, currentNode);
                }

                // 3. Evaluar vecinos
                for (int i = 0; i < _directions.Length; i++)
                {
                    (int dx, int dy) = _directions[i];
                    int neighborX = currentNode.X + dx;
                    int neighborY = currentNode.Y + dy;
                    int movementCost = (i < 4) ? 10 : 14; // 10 para Cardinales, 14 para Diagonales (simulando √2 * 10)

                    // Si el vecino está fuera de límites o no es caminable, ignorar
                    if (!arena.IsInsideBounds(neighborX, neighborY) ||
                        !arena.Grid[neighborY][neighborX].IsWalkable(troop))
                    {
                        continue;
                    }

                    var neighbor = GetNode(neighborX, neighborY, allNodes);

                    // Si ya fue evaluado, ignorar
                    if (closedSet.Contains(neighbor)) continue;

                    // Calcular el nuevo GCost
                    int newGCost = currentNode.GCost + movementCost;

                    // Si es un camino mejor O no ha sido visitado todavía
                    if (newGCost < neighbor.GCost || !openSet.Contains(neighbor))
                    {
                        neighbor.GCost = newGCost;
                        neighbor.HCost = CalculateHCost(neighborX, neighborY, targetX, targetY);
                        neighbor.Parent = currentNode;

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            // Si el bucle termina, no se encontró una ruta
            return [];
        }

        /// <summary>
        /// Calcula la distancia de Manhattan (usada como heurística para el A*).
        /// </summary>
        private static int CalculateHCost(int startX, int startY, int targetX, int targetY)
        {
            // Se usa distancia de Manhattan o diagonal, multiplicada por 10 (costo de un movimiento cardinal)
            int dstX = Math.Abs(startX - targetX);
            int dstY = Math.Abs(startY - targetY);

            // Distancia Diagonal (preferida para movimientos de 8 direcciones)
            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }

        /// <summary>
        /// Reconstruye la ruta desde el nodo final hasta el inicial.
        /// </summary>
        private static List<(int X, int Y)> RetracePath(PathfindingNode startNode, PathfindingNode endNode)
        {
            var path = new List<(int X, int Y)>();
            var currentNode = endNode;

            // Se detiene antes del nodo inicial, ya que la tropa ya está allí.
            while (currentNode != startNode && currentNode.Parent != null)
            {
                // Añadir el paso AL INICIO de la lista (para que el orden sea Start -> End)
                path.Insert(0, (currentNode.X, currentNode.Y));
                currentNode = currentNode.Parent;
            }

            return path;
        }

        /// <summary>
        /// Helper para obtener un nodo de la caché o crear uno nuevo.
        /// </summary>
        private static PathfindingNode GetNode(int x, int y, Dictionary<(int, int), PathfindingNode> allNodes)
        {
            if (!allNodes.TryGetValue((x, y), out var node))
            {
                node = new PathfindingNode(x, y);
                allNodes.Add((x, y), node);
            }
            return node;
        }

        /// <summary>
        /// Determina si dos coordenadas están adyacentes (cardinal o diagonal).
        /// </summary>
        private static bool IsAdjacent(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1 - x2) <= 1 && Math.Abs(y1 - y2) <= 1;
        }
    }
}