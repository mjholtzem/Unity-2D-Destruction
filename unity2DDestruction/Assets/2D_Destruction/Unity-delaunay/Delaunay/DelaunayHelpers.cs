using UnityEngine;
using System;
using System.Collections.Generic;
using Delaunay.Geo;
using Delaunay.LR;

namespace Delaunay
{	

	public class Node
	{
		public static Stack<Node> pool = new Stack<Node> ();
		
		public Node parent;
		public int treeSize;
	}

	public enum KruskalType
	{
		MINIMUM,
		MAXIMUM
	}

	public static class DelaunayHelpers
	{
		public static List<LineSegment> VisibleLineSegments (List<Edge> edges)
		{
			List<LineSegment> segments = new List<LineSegment> ();
			
			for (int i = 0; i<edges.Count; i++) {
				Edge edge = edges [i];
				if (edge.visible) {
					Nullable<Vector2> p1 = edge.clippedEnds [Side.LEFT];
					Nullable<Vector2> p2 = edge.clippedEnds [Side.RIGHT];
					segments.Add (new LineSegment (p1, p2));
				}
			}
			
			return segments;
		}

		public static List<Edge> SelectEdgesForSitePoint (Vector2 coord, List<Edge> edgesToTest)
		{
			return edgesToTest.FindAll (delegate (Edge edge) {
				return ((edge.leftSite != null && edge.leftSite.Coord == coord)
					|| (edge.rightSite != null && edge.rightSite.Coord == coord));
			});
		}

		public static List<Edge> SelectNonIntersectingEdges (/*keepOutMask:BitmapData,*/List<Edge> edgesToTest)
		{
//			if (keepOutMask == null)
//			{
			return edgesToTest;
//			}
			
//			var zeroPoint:Point = new Point();
//			return edgesToTest.filter(myTest);
//			
//			function myTest(edge:Edge, index:int, vector:Vector.<Edge>):Boolean
//			{
//				var delaunayLineBmp:BitmapData = edge.makeDelaunayLineBmp();
//				var notIntersecting:Boolean = !(keepOutMask.hitTest(zeroPoint, 1, delaunayLineBmp, zeroPoint, 1));
//				delaunayLineBmp.dispose();
//				return notIntersecting;
//			}
		}

		public static List<LineSegment> DelaunayLinesForEdges (List<Edge> edges)
		{
			List<LineSegment> segments = new List<LineSegment> ();
			Edge edge;
			for (int i = 0; i < edges.Count; i++) {
				edge = edges [i];
				segments.Add (edge.DelaunayLine ());
			}
			return segments;
		}
		
		/**
		*  Kruskal's spanning tree algorithm with union-find
		 * Skiena: The Algorithm Design Manual, p. 196ff
		 * Note: the sites are implied: they consist of the end points of the line segments
		*/
		public static List<LineSegment> Kruskal (List<LineSegment> lineSegments, KruskalType type = KruskalType.MINIMUM)
		{
			Dictionary<Nullable<Vector2>,Node> nodes = new Dictionary<Nullable<Vector2>,Node> ();
			List<LineSegment> mst = new List<LineSegment> ();
			Stack<Node> nodePool = Node.pool;
			
			switch (type) {
			// note that the compare functions are the reverse of what you'd expect
			// because (see below) we traverse the lineSegments in reverse order for speed
			case KruskalType.MAXIMUM:
				lineSegments.Sort (delegate (LineSegment l1, LineSegment l2) {
					return LineSegment.CompareLengths (l1, l2);
				});
				break;
			default:
				lineSegments.Sort (delegate (LineSegment l1, LineSegment l2) {
					return LineSegment.CompareLengths_MAX (l1, l2);
				});
				break;
			}
			
			for (int i = lineSegments.Count; --i > -1;) {
				LineSegment lineSegment = lineSegments [i];

				Node node0 = null;
				Node rootOfSet0;
				if (!nodes.ContainsKey (lineSegment.p0)) {
					node0 = nodePool.Count > 0 ? nodePool.Pop () : new Node ();
					// intialize the node:
					rootOfSet0 = node0.parent = node0;
					node0.treeSize = 1;
					
					nodes [lineSegment.p0] = node0;
				} else {
					node0 = nodes [lineSegment.p0];
					rootOfSet0 = Find (node0);
				}
				
				Node node1 = null;
				Node rootOfSet1;
				if (!nodes.ContainsKey (lineSegment.p1)) {
					node1 = nodePool.Count > 0 ? nodePool.Pop () : new Node ();
					// intialize the node:
					rootOfSet1 = node1.parent = node1;
					node1.treeSize = 1;
					
					nodes [lineSegment.p1] = node1;
				} else {
					node1 = nodes [lineSegment.p1];
					rootOfSet1 = Find (node1);
				}
				
				if (rootOfSet0 != rootOfSet1) {	// nodes not in same set
					mst.Add (lineSegment);
					
					// merge the two sets:
					int treeSize0 = rootOfSet0.treeSize;
					int treeSize1 = rootOfSet1.treeSize;
					if (treeSize0 >= treeSize1) {
						// set0 absorbs set1:
						rootOfSet1.parent = rootOfSet0;
						rootOfSet0.treeSize += treeSize1;
					} else {
						// set1 absorbs set0:
						rootOfSet0.parent = rootOfSet1;
						rootOfSet1.treeSize += treeSize0;
					}
				}
			}
			foreach (Node node in nodes.Values) {
				nodePool.Push (node);
			}
			
			return mst;
		}

		private static Node Find (Node node)
		{
			if (node.parent == node) {
				return node;
			} else {
				Node root = Find (node.parent);
				// this line is just to speed up subsequent finds by keeping the tree depth low:
				node.parent = root;
				return root;
			}
		}
	}


}