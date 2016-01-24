using UnityEngine;
using System;
using System.Collections.Generic;
using Delaunay.LR;
using Delaunay.Geo;
using Delaunay.Utils;

namespace Delaunay
{

	
	public sealed class Halfedge: Delaunay.Utils.IDisposable
	{
		private static Stack<Halfedge> _pool = new Stack<Halfedge> ();
		public static Halfedge Create (Edge edge, Nullable<Side> lr)
		{
			if (_pool.Count > 0) {
				return _pool.Pop ().Init (edge, lr);
			} else {
				return new Halfedge (edge, lr);
			}
		}
		
		public static Halfedge CreateDummy ()
		{
			return Create (null, null);
		}
		
		public Halfedge edgeListLeftNeighbor, edgeListRightNeighbor;
		public Halfedge nextInPriorityQueue;
		
		public Edge edge;
		public Nullable<Side> leftRight;
		public Vertex vertex;
		
		// the vertex's y-coordinate in the transformed Voronoi space V*
		public float ystar;

		public Halfedge (Edge edge = null, Nullable<Side> lr = null)
		{
			Init (edge, lr);
		}
		
		private Halfedge Init (Edge edge, Nullable<Side> lr)
		{
			this.edge = edge;
			leftRight = lr;
			nextInPriorityQueue = null;
			vertex = null;
			return this;
		}
		
		public override string ToString ()
		{
			return "Halfedge (leftRight: " + leftRight.ToString () + "; vertex: " + vertex.ToString () + ")";
		}
		
		public void Dispose ()
		{
			if (edgeListLeftNeighbor != null || edgeListRightNeighbor != null) {
				// still in EdgeList
				return;
			}
			if (nextInPriorityQueue != null) {
				// still in PriorityQueue
				return;
			}
			edge = null;
			leftRight = null;
			vertex = null;
			_pool.Push (this);
		}
		
		public void ReallyDispose ()
		{
			edgeListLeftNeighbor = null;
			edgeListRightNeighbor = null;
			nextInPriorityQueue = null;
			edge = null;
			leftRight = null;
			vertex = null;
			_pool.Push (this);
		}

		internal bool IsLeftOf (Vector2 p)
		{
			Site topSite;
			bool rightOfSite, above, fast;
			float dxp, dyp, dxs, t1, t2, t3, yl;
			
			topSite = edge.rightSite;
			rightOfSite = p.x > topSite.x;
			if (rightOfSite && this.leftRight == Side.LEFT) {
				return true;
			}
			if (!rightOfSite && this.leftRight == Side.RIGHT) {
				return false;
			}
			
			if (edge.a == 1.0) {
				dyp = p.y - topSite.y;
				dxp = p.x - topSite.x;
				fast = false;
				if ((!rightOfSite && edge.b < 0.0) || (rightOfSite && edge.b >= 0.0)) {
					above = dyp >= edge.b * dxp;	
					fast = above;
				} else {
					above = p.x + p.y * edge.b > edge.c;
					if (edge.b < 0.0) {
						above = !above;
					}
					if (!above) {
						fast = true;
					}
				}
				if (!fast) {
					dxs = topSite.x - edge.leftSite.x;
					above = edge.b * (dxp * dxp - dyp * dyp) <
						dxs * dyp * (1.0 + 2.0 * dxp / dxs + edge.b * edge.b);
					if (edge.b < 0.0) {
						above = !above;
					}
				}
			} else {  /* edge.b == 1.0 */
				yl = edge.c - edge.a * p.x;
				t1 = p.y - yl;
				t2 = p.x - topSite.x;
				t3 = yl - topSite.y;
				above = t1 * t1 > t2 * t2 + t3 * t3;
			}
			return this.leftRight == Side.LEFT ? above : !above;
		}

	}
}