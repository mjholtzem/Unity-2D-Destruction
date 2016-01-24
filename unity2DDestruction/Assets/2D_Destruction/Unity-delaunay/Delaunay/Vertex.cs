using UnityEngine;
using System;
using System.Collections.Generic;
using Delaunay.LR;

namespace Delaunay
{
	
	public sealed class Vertex: ICoord
	{
		public static readonly Vertex VERTEX_AT_INFINITY = new Vertex (float.NaN, float.NaN);
		
		private static Stack<Vertex> _pool = new Stack<Vertex> ();
		private static Vertex Create (float x, float y)
		{
			if (float.IsNaN (x) || float.IsNaN (y)) {
				return VERTEX_AT_INFINITY;
			}
			if (_pool.Count > 0) {
				return _pool.Pop ().Init (x, y);
			} else {
				return new Vertex (x, y);
			}
		}


		private static int _nvertices = 0;
		
		private Vector2 _coord;
		public Vector2 Coord {
			get { return _coord;}
		}
		private int _vertexIndex;
		public int vertexIndex {
			get { return _vertexIndex;}
		}
		
		public Vertex (float x, float y)
		{
			Init (x, y);
		}
		
		private Vertex Init (float x, float y)
		{
			_coord = new Vector2 (x, y);
			return this;
		}
		
		public void Dispose ()
		{
			_pool.Push (this);
		}
		
		public void SetIndex ()
		{
			_vertexIndex = _nvertices++;
		}
		
		public override string ToString ()
		{
			return "Vertex (" + _vertexIndex + ")";
		}

		/**
		 * This is the only way to make a Vertex
		 * 
		 * @param halfedge0
		 * @param halfedge1
		 * @return 
		 * 
		 */
		public static Vertex Intersect (Halfedge halfedge0, Halfedge halfedge1)
		{
			Edge edge0, edge1, edge;
			Halfedge halfedge;
			float determinant, intersectionX, intersectionY;
			bool rightOfSite;
		
			edge0 = halfedge0.edge;
			edge1 = halfedge1.edge;
			if (edge0 == null || edge1 == null) {
				return null;
			}
			if (edge0.rightSite == edge1.rightSite) {
				return null;
			}
		
			determinant = edge0.a * edge1.b - edge0.b * edge1.a;
			if (-1.0e-10 < determinant && determinant < 1.0e-10) {
				// the edges are parallel
				return null;
			}
		
			intersectionX = (edge0.c * edge1.b - edge1.c * edge0.b) / determinant;
			intersectionY = (edge1.c * edge0.a - edge0.c * edge1.a) / determinant;
		
			if (Voronoi.CompareByYThenX (edge0.rightSite, edge1.rightSite) < 0) {
				halfedge = halfedge0;
				edge = edge0;
			} else {
				halfedge = halfedge1;
				edge = edge1;
			}
			rightOfSite = intersectionX >= edge.rightSite.x;
			if ((rightOfSite && halfedge.leftRight == Side.LEFT)
				|| (!rightOfSite && halfedge.leftRight == Side.RIGHT)) {
				return null;
			}
		
			return Vertex.Create (intersectionX, intersectionY);
		}
		
		public float x {
			get { return _coord.x;}
		}
		public float y {
			get{ return _coord.y;}
		}
		
	}
}