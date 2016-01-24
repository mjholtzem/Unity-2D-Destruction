using UnityEngine;
using System;
using System.Collections.Generic;
using Delaunay.Geo;
using Delaunay.LR;

namespace Delaunay
{
	//	import com.nodename.geom.LineSegment;
	//	
	//	import flash.display.BitmapData;
	//	import flash.display.CapsStyle;
	//	import flash.display.Graphics;
	//	import flash.display.LineScaleMode;
	//	import flash.display.Sprite;
	//	import flash.geom.Point;
	//	import flash.geom.Rectangle;
	//	import flash.utils.Dictionary;
		
	/**
		 * The line segment connecting the two Sites is part of the Delaunay triangulation;
		 * the line segment connecting the two Vertices is part of the Voronoi diagram
		 * @author ashaw
		 * 
		 */
	public sealed class Edge
	{
		private static Stack<Edge> _pool = new Stack<Edge> ();

		/**
			 * This is the only way to create a new Edge 
			 * @param site0
			 * @param site1
			 * @return 
			 * 
			 */
		public static Edge CreateBisectingEdge (Site site0, Site site1)
		{
			float dx, dy, absdx, absdy;
			float a, b, c;
			
			dx = site1.x - site0.x;
			dy = site1.y - site0.y;
			absdx = dx > 0 ? dx : -dx;
			absdy = dy > 0 ? dy : -dy;
			c = site0.x * dx + site0.y * dy + (dx * dx + dy * dy) * 0.5f;
			if (absdx > absdy) {
				a = 1.0f;
				b = dy / dx;
				c /= dx;
			} else {
				b = 1.0f;
				a = dx / dy;
				c /= dy;
			}
				
			Edge edge = Edge.Create ();
			
			edge.leftSite = site0;
			edge.rightSite = site1;
			site0.AddEdge (edge);
			site1.AddEdge (edge);
				
			edge._leftVertex = null;
			edge._rightVertex = null;
				
			edge.a = a;
			edge.b = b;
			edge.c = c;
			//trace("createBisectingEdge: a ", edge.a, "b", edge.b, "c", edge.c);
				
			return edge;
		}

		private static Edge Create ()
		{
			Edge edge;
			if (_pool.Count > 0) {
				edge = _pool.Pop ();
				edge.Init ();
			} else {
				edge = new Edge ();
			}
			return edge;
		}
			
		//		private static const LINESPRITE:Sprite = new Sprite();
		//		private static const GRAPHICS:Graphics = LINESPRITE.graphics;
		//		
		//		private var _delaunayLineBmp:BitmapData;
		//		internal function get delaunayLineBmp():BitmapData
		//		{
		//			if (!_delaunayLineBmp)
		//			{
		//				_delaunayLineBmp = makeDelaunayLineBmp();
		//			}
		//			return _delaunayLineBmp;
		//		}
		//		
		//		// making this available to Voronoi; running out of memory in AIR so I cannot cache the bmp
		//		internal function makeDelaunayLineBmp():BitmapData
		//		{
		//			var p0:Point = leftSite.coord;
		//			var p1:Point = rightSite.coord;
		//			
		//			GRAPHICS.clear();
		//			// clear() resets line style back to undefined!
		//			GRAPHICS.lineStyle(0, 0, 1.0, false, LineScaleMode.NONE, CapsStyle.NONE);
		//			GRAPHICS.moveTo(p0.x, p0.y);
		//			GRAPHICS.lineTo(p1.x, p1.y);
		//						
		//			var w:int = int(Math.ceil(Math.max(p0.x, p1.x)));
		//			if (w < 1)
		//			{
		//				w = 1;
		//			}
		//			var h:int = int(Math.ceil(Math.max(p0.y, p1.y)));
		//			if (h < 1)
		//			{
		//				h = 1;
		//			}
		//			var bmp:BitmapData = new BitmapData(w, h, true, 0);
		//			bmp.draw(LINESPRITE);
		//			return bmp;
		//			}

		public LineSegment DelaunayLine ()
		{
			// draw a line connecting the input Sites for which the edge is a bisector:
			return new LineSegment (leftSite.Coord, rightSite.Coord);
		}

		public LineSegment VoronoiEdge ()
		{
			if (!visible)
				return new LineSegment (null, null);
			return new LineSegment (_clippedVertices [Side.LEFT],
	                                         _clippedVertices [Side.RIGHT]);
		}

		private static int _nedges = 0;
			
		public static readonly Edge DELETED = new Edge ();
			
		// the equation of the edge: ax + by = c
		public float a, b, c;
			
		// the two Voronoi vertices that the edge connects
		//		(if one of them is null, the edge extends to infinity)
		private Vertex _leftVertex;
		public Vertex leftVertex {
			get { return _leftVertex;}
		}
		private Vertex _rightVertex;
		public Vertex rightVertex {
			get { return _rightVertex;}
		}
		public Vertex Vertex (Side leftRight)
		{
			return (leftRight == Side.LEFT) ? _leftVertex : _rightVertex;
		}
		public void SetVertex (Side leftRight, Vertex v)
		{
			if (leftRight == Side.LEFT) {
				_leftVertex = v;
			} else {
				_rightVertex = v;
			}
		}
			
		public bool IsPartOfConvexHull ()
		{
			return (_leftVertex == null || _rightVertex == null);
		}
			
		public float SitesDistance ()
		{
			return Vector2.Distance (leftSite.Coord, rightSite.Coord);
		}
			
		public static int CompareSitesDistances_MAX (Edge edge0, Edge edge1)
		{
			float length0 = edge0.SitesDistance ();
			float length1 = edge1.SitesDistance ();
			if (length0 < length1) {
				return 1;
			}
			if (length0 > length1) {
				return -1;
			}
			return 0;
		}
			
		public static int CompareSitesDistances (Edge edge0, Edge edge1)
		{
			return - CompareSitesDistances_MAX (edge0, edge1);
		}
			
		// Once clipVertices() is called, this Dictionary will hold two Points
		// representing the clipped coordinates of the left and right ends...
		private Dictionary<Side,Nullable<Vector2>> _clippedVertices;
		public Dictionary<Side,Nullable<Vector2>> clippedEnds {
			get { return _clippedVertices;}
		}
		// unless the entire Edge is outside the bounds.
		// In that case visible will be false:
		public bool visible {
			get { return _clippedVertices != null;}
		}
			
		// the two input Sites for which this Edge is a bisector:
		private Dictionary<Side,Site> _sites;
		public Site leftSite {
			get{ return _sites [Side.LEFT];}
			set{ _sites [Side.LEFT] = value;}
				
		}
		public Site rightSite {
			get { return _sites [Side.RIGHT];}
			set { _sites [Side.RIGHT] = value;}			
		}

		public Site Site (Side leftRight)
		{
			return _sites [leftRight];
		}
			
		private int _edgeIndex;
			
		public void Dispose ()
		{
//			if (_delaunayLineBmp) {
//				_delaunayLineBmp.Dispose ();
//				_delaunayLineBmp = null;
//			}
			_leftVertex = null;
			_rightVertex = null;
			if (_clippedVertices != null) {
				_clippedVertices [Side.LEFT] = null;
				_clippedVertices [Side.RIGHT] = null;
				_clippedVertices = null;
			}
			_sites [Side.LEFT] = null;
			_sites [Side.RIGHT] = null;
			_sites = null;
				
			_pool.Push (this);
		}

		private Edge ()
		{
			//			if (lock != PrivateConstructorEnforcer)
			//			{
			//				throw new Error("Edge: constructor is private");
			//			}
				
			_edgeIndex = _nedges++;
			Init ();
		}
			
		private void Init ()
		{	
			_sites = new Dictionary<Side,Site> ();
		}
			
		public override string ToString ()
		{
			return "Edge " + _edgeIndex.ToString () + "; sites " + _sites [Side.LEFT].ToString () + ", " + _sites [Side.RIGHT].ToString ()
				+ "; endVertices " + ((_leftVertex != null) ? _leftVertex.vertexIndex.ToString () : "null") + ", "
				+ ((_rightVertex != null) ? _rightVertex.vertexIndex.ToString () : "null") + "::";
		}

		/**
			 * Set _clippedVertices to contain the two ends of the portion of the Voronoi edge that is visible
			 * within the bounds.  If no part of the Edge falls within the bounds, leave _clippedVertices null. 
			 * @param bounds
			 * 
			 */
		public void ClipVertices (Rect bounds)
		{
			float xmin = bounds.xMin;
			float ymin = bounds.yMin;
			float xmax = bounds.xMax;
			float ymax = bounds.yMax;
				
			Vertex vertex0, vertex1;
			float x0, x1, y0, y1;
				
			if (a == 1.0 && b >= 0.0) {
				vertex0 = _rightVertex;
				vertex1 = _leftVertex;
			} else {
				vertex0 = _leftVertex;
				vertex1 = _rightVertex;
			}
			
			if (a == 1.0) {
				y0 = ymin;
				if (vertex0 != null && vertex0.y > ymin) {
					y0 = vertex0.y;
				}
				if (y0 > ymax) {
					return;
				}
				x0 = c - b * y0;
					
				y1 = ymax;
				if (vertex1 != null && vertex1.y < ymax) {
					y1 = vertex1.y;
				}
				if (y1 < ymin) {
					return;
				}
				x1 = c - b * y1;
					
				if ((x0 > xmax && x1 > xmax) || (x0 < xmin && x1 < xmin)) {
					return;
				}
					
				if (x0 > xmax) {
					x0 = xmax;
					y0 = (c - x0) / b;
				} else if (x0 < xmin) {
					x0 = xmin;
					y0 = (c - x0) / b;
				}
					
				if (x1 > xmax) {
					x1 = xmax;
					y1 = (c - x1) / b;
				} else if (x1 < xmin) {
					x1 = xmin;
					y1 = (c - x1) / b;
				}
			} else {
				x0 = xmin;
				if (vertex0 != null && vertex0.x > xmin) {
					x0 = vertex0.x;
				}
				if (x0 > xmax) {
					return;
				}
				y0 = c - a * x0;
					
				x1 = xmax;
				if (vertex1 != null && vertex1.x < xmax) {
					x1 = vertex1.x;
				}
				if (x1 < xmin) {
					return;
				}
				y1 = c - a * x1;
					
				if ((y0 > ymax && y1 > ymax) || (y0 < ymin && y1 < ymin)) {
					return;
				}
					
				if (y0 > ymax) {
					y0 = ymax;
					x0 = (c - y0) / a;
				} else if (y0 < ymin) {
					y0 = ymin;
					x0 = (c - y0) / a;
				}
					
				if (y1 > ymax) {
					y1 = ymax;
					x1 = (c - y1) / a;
				} else if (y1 < ymin) {
					y1 = ymin;
					x1 = (c - y1) / a;
				}
			}

			//			_clippedVertices = new Dictionary(true); // XXX: Weak ref'd dict might be a problem to use standard
			_clippedVertices = new Dictionary<Side,Nullable<Vector2>> ();
			if (vertex0 == _leftVertex) {
				_clippedVertices [Side.LEFT] = new Vector2 (x0, y0);
				_clippedVertices [Side.RIGHT] = new Vector2 (x1, y1);
			} else {
				_clippedVertices [Side.RIGHT] = new Vector2 (x0, y0);
				_clippedVertices [Side.LEFT] = new Vector2 (x1, y1);
			}
		}

	}
}

//class PrivateConstructorEnforcer {}