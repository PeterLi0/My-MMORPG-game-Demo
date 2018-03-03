using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LunaNav
{
    [Serializable]
	public class NavMesh
	{
        public NavMeshParams _param;
        public float[] _orig = new float[3];
        public float _tileWidth;
        public float _tileHeight;
        public int _maxTiles;
        public int _tileLutSize;
        public int _tileLutMask;
        public MeshTile[] _posLookup;
        public MeshTile _nextFree;
        public MeshTile[] _tiles;

        public long _saltBits;
        public long _tileBits;
        public long _polyBits;

	    public static long NullLink = 0xffffffff;
	    public static int TileFreeData = 1;
        public NavMeshParams Param
        {
            get { return _param; }
        }

        public NavMesh()
        {
            _orig = new float[3];
        }

        public Status Init(NavMeshParams param)
        {
            _param = param;
            Array.Copy(param.Orig, _orig, 3);
            _tileWidth = param.TileWidth;
            _tileHeight = param.TileHeight;

            _maxTiles = param.MaxTiles;
            _tileLutSize = (int)Helper.NextPow2(param.MaxTiles/4);
            if (_tileLutSize <= 0) _tileLutSize = 1;
            _tileLutMask = _tileLutSize - 1;

            _tiles = new MeshTile[_maxTiles];
            _posLookup = new MeshTile[_tileLutSize];

            for (int i = 0; i < _tileLutSize; i++)
            {
                _posLookup[i] = new MeshTile();
            }

            _nextFree = null;
            for (int i = _maxTiles-1; i >= 0; i--)
            {
                _tiles[i] = new MeshTile {Salt = 1, Next = _nextFree};
                _nextFree = _tiles[i];
            }

            _tileBits = Helper.Ilog2(Helper.NextPow2(param.MaxTiles));
            _polyBits = Helper.Ilog2(Helper.NextPow2(param.MaxPolys));
            _saltBits = Math.Min(31, 32 - _tileBits - _polyBits);
            if (_saltBits < 10)
            {
                return Status.Failure | Status.InvalidParam;
            }
            return Status.Success;
        }

	    public Status Init(NavMeshBuilder data, int flags)
        {
            if(data.Header.Magic != Helper.NavMeshMagic)
                throw new ArgumentException("Wrong Magic Number");
            if (data.Header.Version != Helper.NavMeshVersion)
                throw new ArgumentException("Wrong Version Number");

            NavMeshParams param = new NavMeshParams();
            Array.Copy(data.Header.BMin, param.Orig, 3);
	        param.TileWidth = data.Header.BMax[0] - data.Header.BMin[0];
	        param.TileHeight = data.Header.BMax[2] - data.Header.BMin[2];
	        param.MaxTiles = 1;
	        param.MaxPolys = data.Header.PolyCount;

	        Status status = Init(param);
	        if ((status & Status.Failure) == Status.Failure)
	            return status;

	        long temp = 0;
	        return AddTile(data, flags, 0, ref temp);
        }

        public Status AddTile(NavMeshBuilder data, int flags, long lastRef, ref long result)
        {
            MeshHeader header = data.Header;
            if(header.Magic != Helper.NavMeshMagic)
                return Status.Failure | Status.WrongMagic;
            if (header.Version != Helper.NavMeshVersion)
                return Status.Failure | Status.WrongVersion;

            if(GetTileAt(header.X, header.Y, header.Layer) != null)
                return Status.Failure;

            MeshTile tile = null;
            if (lastRef == 0)
            {
                if (_nextFree != null)
                {
                    tile = _nextFree;
                    _nextFree = tile.Next;
                    tile.Next = null;
                }
            }
            else
            {
                int tileIndex = (int) DecodePolyIdTile(lastRef);
                if(tileIndex >= _maxTiles)
                    return Status.Failure | Status.OutOfMemory;

                MeshTile target = _tiles[tileIndex];
                MeshTile prev = null;
                tile = _nextFree;
                while (tile != null && tile != target)
                {
                    prev = tile;
                    tile = tile.Next;
                }

                if (tile != target)
                    return Status.Failure | Status.OutOfMemory;

                if (prev != null)
                    _nextFree = tile.Next;
                else
                {
                    prev.Next = tile.Next;
                }

                tile.Salt = DecodePolyIdSalt(lastRef);
            }

            if(tile == null)
                return Status.Failure | Status.OutOfMemory;

            // insert tile into the position
            int h = ComputeTileHash(header.X, header.Y, _tileLutMask);
            tile.Next = _posLookup[h];
            _posLookup[h] = tile;

            tile.Verts = data.NavVerts;
            tile.Polys = data.NavPolys;
            tile.Links = data.NavLinks;
            tile.DetailMeshes = data.NavDMeshes;
            tile.DetailVerts = data.NavDVerts;
            tile.DetailTris = data.NavDTris;
            tile.BVTree = data.NavBvTree;
            tile.OffMeshCons = data.OffMeshCons;

            tile.LinksFreeList = 0;
            tile.Links[header.MaxLinkCount - 1].Next = NullLink;
            for (int i = 0; i < header.MaxLinkCount-1; i++)
            {
                tile.Links[i].Next = i + 1;
            }

            tile.Data = data;
            tile.Header = header;
            tile.Flags = flags;

            ConnectIntLinks(tile);
            BaseOffMeshLinks(tile);

            int MaxNeis = 32;
            MeshTile[] neis = new MeshTile[MaxNeis];
            int nneis;
            nneis = GetTilesAt(header.X, header.Y, ref neis, MaxNeis);
            for (int j = 0; j < nneis; j++)
            {
                MeshTile temp = neis[j];
                if (neis[j] != tile)
                {
                    ConnectExtLinks(ref tile, ref temp, -1);
                    ConnectExtLinks(ref temp, ref tile, -1);
                }
                ConnectExtOffMeshLinks(ref tile, ref temp, -1);
                ConnectExtOffMeshLinks(ref temp, ref tile, -1);
            }

            for (int i = 0; i < 8; i++)
            {
                nneis = GetNeighborTilesAt(header.X, header.Y, i, ref neis, MaxNeis);
                for (int j = 0; j < nneis; j++)
                {
                    MeshTile temp = neis[j];
                    ConnectExtLinks(ref tile, ref temp, i);
                    ConnectExtLinks(ref temp, ref tile, Helper.OppositeTile(i));
                    ConnectExtOffMeshLinks(ref tile, ref temp, i);
                    ConnectExtOffMeshLinks(ref temp, ref tile, Helper.OppositeTile(i));
                }
            }

            result = GetTileRef(tile);

            return Status.Success;
        }

	    private int ComputeTileHash(int x, int y, int mask)
	    {
	        long h1 = 0x8da6b343;
	        long h2 = 0xd8163841;
	        long n = h1*x + h2*y;
	        return (int) (n & mask);
	    }

	    public Status RemoveTile(long refId, out NavMeshBuilder data)
	    {
	        data = null;
            if(refId == 0)
                return Status.Failure | Status.InvalidParam;
	        long tileIndex = DecodePolyIdTile(refId);
	        long tileSalt = DecodePolyIdSalt(refId);
            if(tileIndex >= _maxTiles)
                return Status.Failure | Status.InvalidParam;
	        MeshTile tile = _tiles[tileIndex];
            if(tile.Salt != tileSalt)
                return Status.Failure | Status.InvalidParam;

	        int h = ComputeTileHash(tile.Header.X, tile.Header.Y, _tileLutMask);
	        MeshTile prev = null;
	        MeshTile cur = _posLookup[h];
            while (cur != null)
            {
                if (cur == tile)
                {
                    if (prev != null)
                        prev.Next = cur.Next;
                    else
                    {
                        _posLookup[h] = cur.Next;
                    }
                    break;
                }
                prev = cur;
                cur = cur.Next;
            }

	        int MaxNeis = 32;
            MeshTile[] neis = new MeshTile[MaxNeis];
	        int nneis;

	        nneis = GetTilesAt(tile.Header.X, tile.Header.Y, ref neis, MaxNeis);
	        for (int j = 0; j < nneis; j++)
	        {
	            if (neis[j] == tile) continue;
	            MeshTile temp = neis[j];
                UnconnectExtLinks(ref temp, ref tile);
	        }

	        for (int i = 0; i < 8; i++)
	        {
	            nneis = GetNeighborTilesAt(tile.Header.X, tile.Header.Y, i, ref neis, MaxNeis);
	            for (int j = 0; j < nneis; j++)
	            {
	                MeshTile temp = neis[j];
	                UnconnectExtLinks(ref temp, ref tile);
	            }
	        }

            // reset tile
            if ((tile.Flags & TileFreeData) != 0)
            {
                tile.Data = null;
            }
            else
            {
                data = tile.Data;
            }

	        tile.Header = null;
	        tile.Flags = 0;
	        tile.LinksFreeList = 0;
	        tile.Polys = null;
	        tile.Verts = null;
	        tile.Links = null;
	        tile.DetailMeshes = null;
	        tile.DetailVerts = null;
	        tile.DetailTris = null;
	        tile.BVTree = null;
	        tile.OffMeshCons = null;

	        tile.Salt = (tile.Salt + 1) & ((1 << (int)_saltBits) - 1);
	        if (tile.Salt == 0)
	            tile.Salt++;

	        tile.Next = _nextFree;
	        _nextFree = tile;

            return Status.Success;
	    }

        public void CalcTileLoc(float posx, float posy, float posz, out int tx, out int ty)
        {
            tx = (int) Math.Floor((posx - _orig[0])/_tileWidth);
            ty = (int) Math.Floor((posz - _orig[2])/_tileHeight);
        }

        public MeshTile GetTileAt(int x, int y, int layer)
        {
            int h = ComputeTileHash(x, y, _tileLutMask);
            MeshTile tile = _posLookup[h];
            while (tile != null)
            {
                if (tile.Header != null && tile.Header.X == x && tile.Header.Y == y && tile.Header.Layer == layer)
                    return tile;
                tile = tile.Next;
            }
            return null;
        }

        public int GetTilesAt(int x, int y, ref MeshTile[] tiles, int maxTiles)
        {
            int n = 0;

            int h = ComputeTileHash(x, y, _tileLutMask);
            MeshTile tile = _posLookup[h];
            while (tile != null)
            {
                if (tile.Header != null && tile.Header.X == x && tile.Header.Y == y)
                {
                    if (n < maxTiles)
                        tiles[n++] = tile;
                }
                tile = tile.Next;
            }
            return n;
        }

        public long GetTileRefAt(int x, int y, int layer)
        {
            int h = ComputeTileHash(x, y, _tileLutMask);
            MeshTile tile = _posLookup[h];
            while (tile != null)
            {
                if (tile.Header != null && tile.Header.X == x && tile.Header.Y == y && tile.Header.Layer == layer)
                    return GetTileRef(tile);
                tile = tile.Next;
            }
            return 0;
        }

        public long GetTileRef(MeshTile tile)
        {
            if (tile == null) return 0;
            long it = -1;
            for (int i = 0; i < _tiles.Length; i++)
            {
                if (_tiles[i] == tile)
                    it = i;
            }
            return EncodePolyId(tile.Salt, it, 0);
        }

        public MeshTile GetTileByRef(long refId)
        {
            if (refId == 0)
                return null;

            long tileIndex = DecodePolyIdTile(refId);
            long tileSalt = DecodePolyIdSalt(refId);
            if ((int) tileIndex >= _maxTiles)
                return null;
            MeshTile tile = _tiles[tileIndex];
            if (tile.Salt != tileSalt)
                return null;
            return tile;
        }

        public int GetMaxTiles()
        {
            return _maxTiles;
        }

        public MeshTile GetTile(int i)
        {
            return _tiles[i];
        }

        public Status GetTileAndPolyByRef(long refid, ref MeshTile tile, ref Poly poly)
        {
            if(refid == 0) return Status.Failure;
            long salt, it, ip;
            DecodePolyId(refid, out salt, out it, out ip);
            if(it >= _maxTiles) return Status.Failure|Status.InvalidParam;
            if(_tiles[it].Salt != salt || _tiles[it].Header == null) return Status.Failure | Status.InvalidParam;
            if(ip >= _tiles[it].Header.PolyCount) return Status.Failure | Status.InvalidParam;
            tile = _tiles[it];
            poly = _tiles[it].Polys[ip];
            return Status.Success;
        }

        public void GetTileAndPolyByRefUnsafe(long refId, out MeshTile tile, out Poly poly)
        {
            long salt, it, ip;
            DecodePolyId(refId, out salt, out it, out ip);
            tile = _tiles[it];
            poly = _tiles[it].Polys[ip];
        }

        public bool IsValidPolyRef(long refId)
        {
            if (refId == 0) return false;
            long salt, it, ip;
            DecodePolyId(refId, out salt, out it, out ip);
            if (it >= _maxTiles) return false;
            if (_tiles[it].Salt != salt || _tiles[it].Header == null) return false;
            if (ip >= _tiles[it].Header.PolyCount) return false;
            return true;
        }

        public long GetPolyRefBase(MeshTile tile)
        {
            if (tile == null) return 0;
            long it = -1;
            for (int i = 0; i < _tiles.Length; i++)
            {
                if (_tiles[i] == tile)
                    it = i;
            }
            return EncodePolyId(tile.Salt, it, 0);
        }

        public Status GetOffMeshConnectionPolyEndPoints(long prevRef, long polyRef, ref float[] startPos, ref float[] endPos)
        {
            long salt, it, ip;

            if(polyRef == 0) return Status.Failure;

            DecodePolyId(polyRef, out salt, out it, out ip);
            if (it >= _maxTiles) return Status.Failure | Status.InvalidParam;
            if (_tiles[it].Salt != salt || _tiles[it].Header == null) return Status.Failure | Status.InvalidParam;
            MeshTile tile = _tiles[it];
            if (ip >= _tiles[it].Header.PolyCount) return Status.Failure | Status.InvalidParam;

            Poly poly = tile.Polys[ip];

            if(poly.Type != NavMeshBuilder.PolyTypeOffMeshConnection)
                return Status.Failure;

            int idx0 = 0, idx1 = 1;

            for (long i = poly.FirstLink; i != NullLink; i = tile.Links[i].Next)
            {
                if (tile.Links[i].Edge == 0)
                {
                    if (tile.Links[i].Ref != prevRef)
                    {
                        idx0 = 1;
                        idx1 = 0;
                    }
                    break;
                }
            }

            Array.Copy(tile.Verts, poly.Verts[idx0] * 3, startPos, 0, 3);
            Array.Copy(tile.Verts, poly.Verts[idx1]*3, endPos, 0, 3);

            return Status.Success;
        }

        public OffMeshConnection GetOffMeshConnectionByRef(long refId)
        {
            long salt, it, ip;

            if(refId == 0) return null;

            DecodePolyId(refId, out salt, out it, out ip);
            if (it >= _maxTiles) return null;
            if (_tiles[it].Salt != salt || _tiles[it].Header == null) return null;
            MeshTile tile = _tiles[it];
            if (ip >= _tiles[it].Header.PolyCount) return null;

            Poly poly = tile.Polys[ip];

            if (poly.Type != NavMeshBuilder.PolyTypeOffMeshConnection)
                return null;

            long idx = ip - tile.Header.OffMeshBase;
            return tile.OffMeshCons[idx];
        }

        public Status SetPolyFlags(long refId, int flags)
        {
            long salt, it, ip;

            if (refId == 0) return Status.Failure;

            DecodePolyId(refId, out salt, out it, out ip);
            if (it >= _maxTiles) return Status.Failure | Status.InvalidParam;
            if (_tiles[it].Salt != salt || _tiles[it].Header == null) return Status.Failure | Status.InvalidParam;
            MeshTile tile = _tiles[it];
            if (ip >= _tiles[it].Header.PolyCount) return Status.Failure | Status.InvalidParam;

            Poly poly = tile.Polys[ip];
            poly.Flags = flags;
            return Status.Success;
        }

        public Status GetPolyFlags(long refId, ref int resultFlags)
        {
            long salt, it, ip;

            if (refId == 0) return Status.Failure;

            DecodePolyId(refId, out salt, out it, out ip);
            if (it >= _maxTiles) return Status.Failure | Status.InvalidParam;
            if (_tiles[it].Salt != salt || _tiles[it].Header == null) return Status.Failure | Status.InvalidParam;
            MeshTile tile = _tiles[it];
            if (ip >= _tiles[it].Header.PolyCount) return Status.Failure | Status.InvalidParam;

            Poly poly = tile.Polys[ip];
            resultFlags = poly.Flags;

            return Status.Success;
        }

        public Status SetPolyArea(long refId, short area)
        {
            long salt, it, ip;

            if (refId == 0) return Status.Failure;

            DecodePolyId(refId, out salt, out it, out ip);
            if (it >= _maxTiles) return Status.Failure | Status.InvalidParam;
            if (_tiles[it].Salt != salt || _tiles[it].Header == null) return Status.Failure | Status.InvalidParam;
            MeshTile tile = _tiles[it];
            if (ip >= _tiles[it].Header.PolyCount) return Status.Failure | Status.InvalidParam;

            Poly poly = tile.Polys[ip];
            poly.Area = area;
            return Status.Success;
        }

        public Status GetPolyArea(long refId, ref short resultArea)
        {
            long salt, it, ip;

            if (refId == 0) return Status.Failure;

            DecodePolyId(refId, out salt, out it, out ip);
            if (it >= _maxTiles) return Status.Failure | Status.InvalidParam;
            if (_tiles[it].Salt != salt || _tiles[it].Header == null) return Status.Failure | Status.InvalidParam;
            MeshTile tile = _tiles[it];
            if (ip >= _tiles[it].Header.PolyCount) return Status.Failure | Status.InvalidParam;

            Poly poly = tile.Polys[ip];
            resultArea = poly.Area;

            return Status.Success;
        }

        public Status StoreTileState(MeshTile tile, out TileState tileState)
        {
            tileState = new TileState();
            tileState.Magic = Helper.NavMeshMagic;
            tileState.Version = Helper.NavMeshVersion;
            tileState.Ref = GetTileRef(tile);
            tileState.PolyStates = new PolyState[tile.Header.PolyCount];
            for (int i = 0; i < tile.Header.PolyCount; i++)
            {
                Poly p = tile.Polys[i];
                tileState.PolyStates[i] = new PolyState();
                tileState.PolyStates[i].Flags = p.Flags;
                tileState.PolyStates[i].Area = p.Area;
            }

            return Status.Success;
        }

	    public Status RestoreTileState(MeshTile tile, TileState tileState)
	    {
	        if (tileState.Magic != Helper.NavMeshMagic)
	            return Status.Failure | Status.WrongMagic;
	        if (tileState.Version != Helper.NavMeshVersion)
	            return Status.Failure | Status.WrongVersion;
	        if (tileState.Ref != GetTileRef(tile))
	            return Status.Failure | Status.InvalidParam;

	        for (int i = 0; i < tile.Header.PolyCount; i++)
	        {
	            Poly p = tile.Polys[i];
	            PolyState s = tileState.PolyStates[i];
	            p.Flags = s.Flags;
	            p.Area = s.Area;
	        }

            return Status.Success;
	    }

        public long EncodePolyId(long salt, long it, long ip)
        {
            return (salt << (int)(_polyBits + _tileBits)) | (it << (int)_polyBits) | ip;
        }

        public void DecodePolyId(long refId, out long salt, out long it, out long ip)
        {
            long saltMask = (1 << (int)_saltBits) - 1;
            long tileMask = (1 << (int)_tileBits) - 1;
            long polyMask = (1 << (int)_polyBits) - 1;

            salt = ((refId >> (int)(_polyBits + _tileBits)) & saltMask);
            it = ((refId >> (int)_polyBits) & tileMask);
            ip = (refId & polyMask);
        }

        public long DecodePolyIdSalt(long refId)
        {
            long saltMask = (1 << (int)_saltBits) - 1;
            return ((refId >> (int)(_polyBits + _tileBits)) & saltMask);
        }

        public long DecodePolyIdTile(long refId)
        {
            long tileMask = (1 << (int)_tileBits) - 1;
            return ((refId >> (int)_polyBits) & tileMask);
        }

        public long DecodePolyIdPoly(long refId)
        {
            long polyMask = (1 << (int)_polyBits) - 1;
            return refId & polyMask;
        }

	    private int GetNeighborTilesAt(int x, int y, int side, ref MeshTile[] tiles, int maxTiles)
	    {
	        int nx = x, ny = y;
	        switch (side)
	        {
                case 0:
	                nx++;
	                break;
                case 1:
	                nx++;
	                ny++;
	                break;
                case 2:
	                ny++;
	                break;
                case 3:
	                nx--;
	                ny++;
	                break;
                case 4:
	                nx--;
	                break;
                case 5:
	                nx--;
	                ny--;
	                break;
                case 6:
	                ny--;
	                break;
                case 7:
	                nx++;
	                ny--;
	                break;
	        }

	        return GetTilesAt(nx, ny, ref tiles, maxTiles);
	    }

        private int FindConnectingPolys(float vax, float vay, float vaz, float vbx, float vby, float vbz, MeshTile tile,
                                        int side, ref long[] con, ref float[] conarea, int maxcon)
        {
            if (tile == null) return 0;
            float[] amin = new float[2], amax = new float[2];
            Helper.CalcSlabEndPoints(vax, vay, vaz, vbx, vby, vbz, ref amin, ref amax, side);
            float apos = Helper.GetSlabCoord(vax, vay, vaz, side);

            float[] bmin = new float[2], bmax = new float[2];
            int m = NavMeshBuilder.ExtLink | side;
            int n = 0;
            long baseId = GetPolyRefBase(tile);

            for (int i = 0; i < tile.Header.PolyCount; i++)
            {
                Poly poly = tile.Polys[i];
                int nv = poly.VertCount;
                for (int j = 0; j < nv; j++)
                {
                    if (poly.Neis[j] != m) continue;

                    int vc = poly.Verts[j]*3;
                    int vd = poly.Verts[(j + 1)%nv]*3;
                    float bpos = Helper.GetSlabCoord(tile.Verts[vc + 0], tile.Verts[vc + 1], tile.Verts[vc + 2], side);

                    if (Math.Abs(apos - bpos) > 0.01f)
                        continue;

                    Helper.CalcSlabEndPoints(tile.Verts[vc + 0], tile.Verts[vc + 1], tile.Verts[vc + 2], tile.Verts[vd + 0], tile.Verts[vd + 1], tile.Verts[vd + 2], ref bmin, ref bmax, side);

                    if (!Helper.OverlapSlabs(amin, amax, bmin, bmax, 0.01f, tile.Header.WalkableClimb)) continue;

                    if (n < maxcon)
                    {
                        conarea[n*2 + 0] = Math.Max(amin[0], bmin[0]);
                        conarea[n*2 + 1] = Math.Min(amax[0], bmax[0]);
                        con[n] = baseId | i;
                        n++;
                    }
                    break;
                }
            }
            return n;
        }

        private void ConnectIntLinks(MeshTile tile)
        {
            if (tile == null) return;
            long baseId = GetPolyRefBase(tile);

            for (int i = 0; i < tile.Header.PolyCount; i++)
            {
                Poly poly = tile.Polys[i];
                poly.FirstLink = NullLink;

                if (poly.Type == NavMeshBuilder.PolyTypeOffMeshConnection)
                    continue;

                for (int j = poly.VertCount-1; j >= 0; j--)
                {
                    if (poly.Neis[j] == 0 || (poly.Neis[j] & NavMeshBuilder.ExtLink) != 0) continue;

                    long idx = AllocLink(tile);
                    if (idx != NullLink)
                    {
                        Link link = tile.Links[idx];
                        link.Ref = baseId | (poly.Neis[j] - 1);
                        link.Edge = (short) j;
                        link.Side = 0xff;
                        link.BMin = link.BMax = 0;

                        link.Next = poly.FirstLink;
                        poly.FirstLink = idx;
                    }
                }
            }
        }

        private void BaseOffMeshLinks(MeshTile tile)
        {
            if (tile == null) return;
            long baseId = GetPolyRefBase(tile);

            for (int i = 0; i < tile.Header.OffMeshConCount; i++)
            {
                OffMeshConnection con = tile.OffMeshCons[i];
                Poly poly = tile.Polys[con.Poly];

                float[] ext = {con.Rad, tile.Header.WalkableClimb, con.Rad};
                int p = 0;
                float[] nearestPt = new float[3];
                long refId = FindNearestPolyInTile(tile, con.Pos[p + 0], con.Pos[p + 1], con.Pos[p + 2], ext[0], ext[1],
                                                   ext[2], ref nearestPt);
                if (refId <= 0) continue;
                if (((nearestPt[0] - con.Pos[p + 0])*(nearestPt[0] - con.Pos[p + 0])) +
                    ((nearestPt[2] - con.Pos[p + 2])*(nearestPt[2] - con.Pos[p + 2])) > (con.Rad*con.Rad))
                {
                    continue;
                }

                int v = poly.Verts[0]*3;
                Array.Copy(nearestPt, 0, tile.Verts, v, 3);

                long idx = AllocLink(tile);
                if (idx != NullLink)
                {
                    Link link = tile.Links[idx];
                    link.Ref = refId;
                    link.Edge = 0;
                    link.Side = 0xff;
                    link.BMin = link.BMax = 0;
                    link.Next = poly.FirstLink;
                    poly.FirstLink = idx;
                }

                long tidx = AllocLink(tile);
                if (tidx != NullLink)
                {
                    int landPolyIdx = (int) DecodePolyIdTile(refId);
                    Poly landPoly = tile.Polys[landPolyIdx];
                    Link link = tile.Links[tidx];
                    link.Ref = baseId | con.Poly;
                    link.Edge = 0xff;
                    link.Side = 0xff;
                    link.BMin = link.BMax = 0;
                    link.Next = landPoly.FirstLink;
                    landPoly.FirstLink = tidx;
                }
            }
        }

        private void ConnectExtLinks(ref MeshTile tile, ref MeshTile target, int side)
        {
            if (tile == null) return;

            for (int i = 0; i < tile.Header.PolyCount; i++)
            {
                Poly poly = tile.Polys[i];

                int nv = poly.VertCount;
                for (int j = 0; j < nv; j++)
                {
                    if ((poly.Neis[j] & NavMeshBuilder.ExtLink) == 0)
                    {
                        continue;
                    }

                    int dir = (int) (poly.Neis[j] & 0xff);
                    if (side != -1 && dir != side)
                        continue;

                    int va = poly.Verts[j]*3;
                    int vb = poly.Verts[(j + 1)%nv]*3;
                    long[] nei = new long[4];
                    float[] neia = new float[4*2];
                    int nnei = FindConnectingPolys(tile.Verts[va + 0], tile.Verts[va + 1], tile.Verts[va + 2],
                                                   tile.Verts[vb + 0], tile.Verts[vb + 1], tile.Verts[vb + 2], target,
                                                   Helper.OppositeTile(dir), ref nei, ref neia, 4);
                    for (int k = 0; k < nnei; k++)
                    {
                        long idx = AllocLink(tile);
                        if (idx != NullLink)
                        {
                            Link link = tile.Links[idx];
                            link.Ref = nei[k];
                            link.Edge = (short)j;
                            link.Side = (short) dir;

                            link.Next = poly.FirstLink;
                            poly.FirstLink = idx;

                            if (dir == 0 || dir == 4)
                            {
                                float tmin = (neia[k*2 + 0] - tile.Verts[va + 2])/
                                             (tile.Verts[vb + 2] - tile.Verts[va + 2]);
                                float tmax = (neia[k * 2 + 1] - tile.Verts[va + 2]) /
                                             (tile.Verts[vb + 2] - tile.Verts[va + 2]);
                                if (tmin > tmax)
                                {
                                    float temp = tmin;
                                    tmin = tmax;
                                    tmax = temp;
                                }
                                link.BMin = (short) (Math.Min(1.0f, Math.Max(tmin, 0.0f))*255.0f);
                                link.BMax = (short) (Math.Min(1.0f, Math.Max(tmax, 0.0f))*255.0f);
                            }
                            else if (dir == 2 || dir == 6)
                            {
                                float tmin = (neia[k * 2 + 0] - tile.Verts[va + 0]) /
                                             (tile.Verts[vb + 0] - tile.Verts[va + 0]);
                                float tmax = (neia[k * 2 + 1] - tile.Verts[va + 0]) /
                                             (tile.Verts[vb + 0] - tile.Verts[va + 0]);
                                if (tmin > tmax)
                                {
                                    float temp = tmin;
                                    tmin = tmax;
                                    tmax = temp;
                                }
                                link.BMin = (short)(Math.Min(1.0f, Math.Max(tmin, 0.0f)) * 255.0f);
                                link.BMax = (short)(Math.Min(1.0f, Math.Max(tmax, 0.0f)) * 255.0f);                                
                            }
                        }
                    }
                }
            }
        }

	    private long AllocLink(MeshTile tile)
	    {
	        if (tile.LinksFreeList == NullLink)
	            return NullLink;
	        long link = tile.LinksFreeList;
	        tile.LinksFreeList = tile.Links[link].Next;
	        return link;
	    }

	    private void ConnectExtOffMeshLinks(ref MeshTile tile, ref MeshTile target, int side)
	    {
	        if (tile == null) return;

	        short oppositeSide = (side == -1) ? (short)0xff : (short) Helper.OppositeTile(side);
	        for (int i = 0; i < target.Header.OffMeshConCount; i++)
	        {
	            OffMeshConnection targetCon = target.OffMeshCons[i];
	            if (targetCon.Side != oppositeSide) continue;

	            Poly targetPoly = target.Polys[targetCon.Poly];
	            if (targetPoly.FirstLink == NullLink)
	                continue;

	            float[] ext = {targetCon.Rad, target.Header.WalkableClimb, targetCon.Rad};

	            int p = 3;
                float[] nearestPt = new float[3];
	            long refId = FindNearestPolyInTile(tile, targetCon.Pos[p + 0], targetCon.Pos[p + 1], targetCon.Pos[p + 2],
	                                               ext[0], ext[1], ext[2], ref nearestPt);
	            if (refId <= 0)
	                continue;

                if(((nearestPt[0]-targetCon.Pos[p + 0])*(nearestPt[0]-targetCon.Pos[p + 0]))+((nearestPt[2]-targetCon.Pos[p + 2])*(nearestPt[2]-targetCon.Pos[p + 2])) > (targetCon.Rad*targetCon.Rad))
                    continue;

	            int v = targetPoly.Verts[1]*3;
                Array.Copy(nearestPt, 0, target.Verts, v, 3);

	            long idx = AllocLink(target);
                if (idx != NullLink)
                {
                    Link link = target.Links[idx];
                    link.Ref = refId;
                    link.Edge = 1;
                    link.Side = oppositeSide;
                    link.BMin = link.BMax = 0;

                    link.Next = targetPoly.FirstLink;
                    targetPoly.FirstLink = idx;
                }

                if ((targetCon.Flags & NavMeshBuilder.OffMeshConBiDir) != 0)
                {
                    long tidx = AllocLink(tile);
                    if (tidx != NullLink)
                    {
                        int landPolyIdx = (int) DecodePolyIdPoly(refId);
                        Poly landPoly = tile.Polys[landPolyIdx];
                        Link link = tile.Links[tidx];
                        link.Ref = GetPolyRefBase(target) | (targetCon.Poly);
                        link.Edge = 0xff;
                        link.Side = side == -1 ? (short)0xff : (short)side;
                        link.BMin = link.BMax = 0;
                        link.Next = landPoly.FirstLink;
                        landPoly.FirstLink = tidx;
                    }
                }
	        }
	    }

        private void UnconnectExtLinks(ref MeshTile tile, ref MeshTile target)
        {
            if (tile == null || target == null) return;

            long targetNum = DecodePolyIdTile(GetTileRef(target));

            for (int i = 0; i < tile.Header.PolyCount; i++)
            {
                Poly poly = tile.Polys[i];
                long j = poly.FirstLink;
                long pj = NullLink;
                while (j != NullLink)
                {
                    if (tile.Links[j].Side != 0xff && DecodePolyIdTile(tile.Links[j].Ref) == targetNum)
                    {
                        long nj = tile.Links[j].Next;
                        if (pj == NullLink)
                            poly.FirstLink = nj;
                        else
                        {
                            tile.Links[pj].Next = nj;
                        }
                        FreeLink(ref tile, j);
                        j = nj;
                    }
                    else
                    {
                        pj = j;
                        j = tile.Links[j].Next;
                    }
                }
            }
        }

	    private void FreeLink(ref MeshTile tile, long link)
	    {
	        tile.Links[link].Next = tile.LinksFreeList;
	        tile.LinksFreeList = link;
	    }

	    private int QueryPolygonsInTile(MeshTile tile, float qminx, float qminy, float qminz, float qmaxx, float qmaxy,
                                        float qmaxz, ref long[] polys, int maxPolys)
        {
            if (tile.BVTree != null)
            {
                int node = 0;
                int end = tile.Header.BVNodeCount;
                float[] tbmin = tile.Header.BMin;
                float[] tbmax = tile.Header.BMax;
                float qfac = tile.Header.BVQuantFactor;

                int[] bmin = new int[3], bmax = new int[3];

                float minx = Math.Min(tbmax[0], Math.Max(qminx, tbmin[0])) - tbmin[0];
                float miny = Math.Min(tbmax[1], Math.Max(qminy, tbmin[1])) - tbmin[1];
                float minz = Math.Min(tbmax[2], Math.Max(qminz, tbmin[2])) - tbmin[2];
                float maxx = Math.Min(tbmax[0], Math.Max(qmaxx, tbmin[0])) - tbmin[0];
                float maxy = Math.Min(tbmax[1], Math.Max(qmaxy, tbmin[1])) - tbmin[1];
                float maxz = Math.Min(tbmax[2], Math.Max(qmaxz, tbmin[2])) - tbmin[2];

                bmin[0] = (int)(qfac * minx) & 0xfffe;
                bmin[1] = (int)(qfac * miny) & 0xfffe;
                bmin[2] = (int)(qfac * minz) & 0xfffe;
                bmax[0] = (int)(qfac * maxx+1) |1;
                bmax[1] = (int)(qfac * maxy+1) |1;
                bmax[2] = (int)(qfac * maxz+1) |1;

                long baseId = GetPolyRefBase(tile);
                int n = 0;
                while (node < end)
                {
                    bool overlap = Helper.OverlapQuantBounds(bmin, bmax, tile.BVTree[node].BMin, tile.BVTree[node].BMax);
                    bool isLeafNode = tile.BVTree[node].I >= 0;

                    if (isLeafNode && overlap)
                    {
                        if (n < maxPolys)
                            polys[n++] = baseId | tile.BVTree[node].I;
                    }

                    if (overlap || isLeafNode)
                        node++;
                    else
                    {
                        int escapeIndex = -tile.BVTree[node].I;
                        node += escapeIndex;
                    }
                }
                return n;
            }
            else
            {
                float[] bmin = new float[3], bmax = new float[3];
                int n = 0;
                long baseId = GetPolyRefBase(tile);
                for (int i = 0; i < tile.Header.PolyCount; i++)
                {
                    Poly p = tile.Polys[i];
                    if (p.Type == NavMeshBuilder.PolyTypeOffMeshConnection)
                        continue;

                    int v = p.Verts[0*3];
                    Array.Copy(tile.Verts, v, bmin, 0, 3);
                    Array.Copy(tile.Verts, v, bmax, 0, 3);
                    for (int j = 1; j < p.VertCount; j++)
                    {
                        v = p.Verts[j]*3;
                        Helper.VMin(ref bmin, tile.Verts[v + 0], tile.Verts[v + 1], tile.Verts[v + 2]);
                        Helper.VMax(ref bmax, tile.Verts[v + 0], tile.Verts[v + 1], tile.Verts[v + 2]);
                    }
                    if (Helper.OverlapBounds(qminx, qminy, qminz, qmaxx, qmaxy, qmaxz, bmin[0], bmin[1], bmin[2],
                                             bmax[0], bmax[1], bmax[2]))
                    {
                        if (n < maxPolys)
                            polys[n++] = baseId | i;
                    }
                }
                return n;
            }
        }

        private long FindNearestPolyInTile(MeshTile tile, float centerx, float centery, float centerz, float extentsx, float extentsy, float extentsz, ref float[] nearestPt)
        {
            float[] bmin, bmax;
            bmin = Helper.VSub(centerx, centery, centerz, extentsx, extentsy, extentsz);
            bmax = Helper.VAdd(centerx, centery, centerz, extentsx, extentsy, extentsz);

            long[] polys = new long[128];
            int polyCount = QueryPolygonsInTile(tile, bmin[0], bmin[1], bmin[2], bmax[0], bmax[1], bmax[2], ref polys, 128);

            long nearest = 0;
            float nearestDistanceSqr = float.MaxValue;
            for (int i = 0; i < polyCount; i++)
            {
                long refId = polys[i];
                float[] closestPtPoly = new float[3];
                ClosestPointOnPolyInTile(tile, DecodePolyIdPoly(refId), centerx, centery, centerz, ref closestPtPoly);
                float d = Helper.VDistSqr(centerx, centery, centerz, closestPtPoly[0], closestPtPoly[1], closestPtPoly[2]);
                if (d < nearestDistanceSqr)
                {
                    if(nearestPt != null)
                        Array.Copy(closestPtPoly, nearestPt, 3);
                    nearestDistanceSqr = d;
                    nearest = refId;
                }
            }
            return nearest;
        }

        private void ClosestPointOnPolyInTile(MeshTile tile, long ip, float posx, float posy, float posz, ref float[] closestPt)
        {
            Poly poly = tile.Polys[ip];
            if (poly.Type == NavMeshBuilder.PolyTypeOffMeshConnection)
            {
                int v0 = poly.Verts[0]*3;
                int v1 = poly.Verts[1]*3;
                float d0 = Helper.VDist(posx, posy, posz, tile.Verts[v0 + 0], tile.Verts[v0 + 1], tile.Verts[v0 + 2]);
                float d1 = Helper.VDist(posx, posy, posz, tile.Verts[v1 + 0], tile.Verts[v1 + 1], tile.Verts[v1 + 2]);
                float u = d0/(d0 + d1);
                Helper.VLerp(ref closestPt, tile.Verts[v0 + 0], tile.Verts[v0 + 1], tile.Verts[v0 + 2], tile.Verts[v1 + 0], tile.Verts[v1 + 1], tile.Verts[v1 + 2], u);
                return;
            }

            PolyDetail pd = tile.DetailMeshes[ip];

            float[] verts = new float[NavMeshBuilder.VertsPerPoly*3];
            float[] edged = new float[NavMeshBuilder.VertsPerPoly];
            float[] edget = new float[NavMeshBuilder.VertsPerPoly];
            int nv = poly.VertCount;
            for (int i = 0; i < nv; i++)
            {
                Array.Copy(tile.Verts, poly.Verts[i]*3, verts, i*3, 3);
            }

            closestPt[0] = posx;
            closestPt[1] = posy;
            closestPt[2] = posz;

            if (!Helper.DistancePtPolyEdgesSqr(posx, posy, posz, verts, nv, ref edged, ref edget))
            {
                float dmin = float.MaxValue;
                int imin = -1;
                for (int i = 0; i < nv; i++)
                {
                    if (edged[i] < dmin)
                    {
                        dmin = edged[i];
                        imin = i;
                    }
                }
                int va = imin*3;
                int vb = ((imin + 1)%nv)*3;
                Helper.VLerp(ref closestPt, verts[va + 0], verts[va + 1], verts[va + 2], verts[vb + 0], verts[vb + 1], verts[vb + 2], edget[imin]);
            }

            for (int j = 0; j < pd.TriCount; j++)
            {
                int t = (int)(pd.TriBase + j)*4;
                float[] v = new float[9];
                for (int k = 0; k < 3; k++)
                {
                    if (tile.DetailTris[t + k] < poly.VertCount)
                    {
                        Array.Copy(tile.Verts, poly.Verts[tile.DetailTris[t + k]] * 3, v, k*3, 3);
                        //v[k] = tile.Verts[poly.Verts[tile.DetailTris[t + k]]*3];
                    }
                    else
                    {
                        Array.Copy(tile.DetailVerts, (pd.VertBase + (tile.DetailTris[t + k] - poly.VertCount)) * 3, v, k*3, 3);
                        //v[k] = tile.DetailVerts[(pd.VertBase + (tile.DetailTris[t + k] - poly.VertCount))*3];
                    }
                }
                float h = 0;
                if (Helper.ClosestHeightPointTriangle(posx, posy, posz, v[0], v[1], v[2], v[3], v[4], v[5], v[6], v[7], v[8], ref h))
                {
                    closestPt[1] = h;
                    break;
                }
            }
        }
	}
}
