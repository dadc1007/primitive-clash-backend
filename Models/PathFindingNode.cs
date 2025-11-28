namespace PrimitiveClash.Backend.Models
{
    public class PathfindingNode(int x, int y)
    {
        public int X { get; } = x;
        public int Y { get; } = y;

        // G Cost: Coste real desde el inicio hasta este nodo.
        public int GCost { get; set; }

        // H Cost: Coste heurístico estimado desde este nodo hasta el destino (manhattan/euclidean distance).
        public int HCost { get; set; }

        // F Cost: GCost + HCost (Coste total estimado).
        public int FCost => GCost + HCost;

        // Referencia al nodo anterior para reconstruir el camino.
        public PathfindingNode? Parent { get; set; }

        // Usado para HashSets/Dictionaries para la búsqueda rápida.
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override bool Equals(object? obj)
        {
            if (obj is PathfindingNode other)
            {
                return X == other.X && Y == other.Y;
            }
            return false;
        }
    }
}