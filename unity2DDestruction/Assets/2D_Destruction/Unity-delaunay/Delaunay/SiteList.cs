using UnityEngine;
using System;
using System.Collections.Generic;
using Delaunay.Geo;
using Delaunay.Utils;

namespace Delaunay
{

	public sealed class SiteList: Utils.IDisposable
	{
		public List<Site> _sites;
		private int _currentIndex;
		
		private bool _sorted;
		
		public SiteList ()
		{
			_sites = new List<Site> ();
			_sorted = false;
		}
		
		public void Dispose ()
		{
			if (_sites != null) {
				for (int i = 0; i < _sites.Count; i++) {
					Site site = _sites [i];
					site.Dispose ();
				}
				_sites.Clear ();
				_sites = null;
			}
		}
		
		public int Add (Site site)
		{
			_sorted = false;
			_sites.Add (site);
			return _sites.Count;
		}
		
		public int Count {
			get { return _sites.Count;}
		}
		
		public Site Next ()
		{
			if (_sorted == false) {
				UnityEngine.Debug.LogError ("SiteList::next():  sites have not been sorted");
			}
			if (_currentIndex < _sites.Count) {
				return _sites [_currentIndex++];
			} else {
				return null;
			}
		}

		internal Rect GetSitesBounds ()
		{
			if (_sorted == false) {
				Site.SortSites (_sites);
				_currentIndex = 0;
				_sorted = true;
			}
			float xmin, xmax, ymin, ymax;
			if (_sites.Count == 0) {
				return new Rect (0, 0, 0, 0);
			}
			xmin = float.MaxValue;
			xmax = float.MinValue;
			for (int i = 0; i<_sites.Count; i++) {
				Site site = _sites [i];
				if (site.x < xmin) {
					xmin = site.x;
				}
				if (site.x > xmax) {
					xmax = site.x;
				}
			}
			// here's where we assume that the sites have been sorted on y:
			ymin = _sites [0].y;
			ymax = _sites [_sites.Count - 1].y;
			
			return new Rect (xmin, ymin, xmax - xmin, ymax - ymin);
		}

		public List<uint> SiteColors (/*BitmapData referenceImage = null*/)
		{
			List<uint> colors = new List<uint> ();
			Site site;
			for (int i = 0; i< _sites.Count; i++) {
				site = _sites [i];
				colors.Add (/*referenceImage ? referenceImage.getPixel(site.x, site.y) :*/site.color);
			}
			return colors;
		}

		public List<Vector2> SiteCoords ()
		{
			List<Vector2> coords = new List<Vector2> ();
			Site site;
			for (int i = 0; i<_sites.Count; i++) {
				site = _sites [i];
				coords.Add (site.Coord);
			}
			return coords;
		}

		/**
		 * 
		 * @return the largest circle centered at each site that fits in its region;
		 * if the region is infinite, return a circle of radius 0.
		 * 
		 */
		public List<Circle> Circles ()
		{
			List<Circle> circles = new List<Circle> ();
			Site site;
			for (int i = 0; i<_sites.Count; i++) {
				site = _sites [i];
				float radius = 0f;
				Edge nearestEdge = site.NearestEdge ();
				
				if (!nearestEdge.IsPartOfConvexHull ()) {
					radius = nearestEdge.SitesDistance () * 0.5f;
				}
				circles.Add (new Circle (site.x, site.y, radius));
			}
			return circles;
		}

		public List<List<Vector2>> Regions (Rect plotBounds)
		{
			List<List<Vector2>> regions = new List<List<Vector2>> ();
			Site site;
			for (int i = 0; i< _sites.Count; i++) {
				site = _sites [i];
				regions.Add (site.Region (plotBounds));
			}
			return regions;
		}

		/**
		 * 
		 * @param proximityMap a BitmapData whose regions are filled with the site index values; see PlanePointsCanvas::fillRegions()
		 * @param x
		 * @param y
		 * @return coordinates of nearest Site to (x, y)
		 * 
		 */
		public Nullable<Vector2> NearestSitePoint (/*proximityMap:BitmapData,*/float x, float y)
		{
//			uint index = proximityMap.getPixel(x, y);
//			if (index > _sites.length - 1)
//			{
			return null;
//			}
//			return _sites[index].coord;
		}
		
	}
}