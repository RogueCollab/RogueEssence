using System;
using System.Collections.Generic;
using System.Linq;
using RogueElements;
using RogueEssence.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Dungeon
{
    public struct HitboxHit
    {
        public bool Explode;
        public Loc Loc;

        public HitboxHit(Loc loc, bool explode)
        {
            Explode = explode;
            Loc = loc;
        }
    }

    public abstract class Hitbox : IDrawableSprite
    {
        public enum AreaLimit
        {
            Full,
            Cone,
            Sides
        }
        public enum TargetHitType
        {
            None,
            Tile,
            Burst
        }

        public Character User;
        public abstract bool Finished { get; }

        public FiniteEmitter TileEmitter;
        public int LagBehindTime;

        public StablePriorityQueue<int, Loc> TilesToEmit;
        public StablePriorityQueue<int, Loc> TilesToHit;

        public Loc MapLoc { get; protected set; }
        public int LocHeight { get; protected set; }

        protected FrameTick time;
        private long leftoverTick;

        public Hitbox(Character user, Loc mapLoc, FiniteEmitter tileEmitter, int delay)
        {
            User = user;
            MapLoc = mapLoc;
            TileEmitter = (FiniteEmitter)tileEmitter.Clone();
            TilesToEmit = new StablePriorityQueue<int, Loc>();
            TilesToHit = new StablePriorityQueue<int, Loc>();
            LagBehindTime = delay;
        }

        public virtual void Update(FrameTick elapsedTime)
        {
            time += elapsedTime;

            while (TilesToEmit.Count > 0 && time >= TilesToEmit.FrontPriority())
            {
                int priority = TilesToEmit.FrontPriority();
                Loc tile = TilesToEmit.Dequeue();

                FiniteEmitter tileEmitter = (FiniteEmitter)TileEmitter.Clone();
                tileEmitter.SetupEmit(tile * GraphicsManager.TileSize, User.MapLoc, User.CharDir);
                DungeonScene.Instance.CreateAnim(tileEmitter, DrawLayer.NoDraw);
            }
        }

        protected long UpdateTick(FrameTick elapsedTime, int speed)
        {
            long add = (speed * GraphicsManager.TileSize * elapsedTime.Ticks + leftoverTick) / FrameTick.FrameToTick(GraphicsManager.MAX_FPS);
            leftoverTick = (speed * GraphicsManager.TileSize * elapsedTime.Ticks + leftoverTick) % FrameTick.FrameToTick(GraphicsManager.MAX_FPS);
            return add;
        }

        public static bool IsInCircleSquareHitbox(Loc loc, Loc origin, int radius, int squareRadius, AreaLimit limit, Dir8 dir)
        {
            //check if inside circle and square
            if ((origin - loc).DistSquared() > radius * radius)
                return false;

            return IsInSquareHitbox(loc, origin, squareRadius, limit, dir);
        }

        public static bool IsInSquareHitbox(Loc loc, Loc origin, int squareRadius, AreaLimit limit, Dir8 dir)
        {
            if (loc.X < origin.X - squareRadius || loc.X > origin.X + squareRadius)
                return false;
            if (loc.Y < origin.Y - squareRadius || loc.Y > origin.Y + squareRadius)
                return false;

            if (limit == AreaLimit.Cone)
            {
                //check if it's inside the cone

                //get line diff
                Loc diff = loc - origin;

                //get cone vectors
                Dir8 cone1 = DirExt.AddAngles(dir, Dir8.DownRight);
                Dir8 cone2 = DirExt.AddAngles(dir, Dir8.DownLeft);

                //get vector orthogonal1 to first cone line (aka, -second cone line)
                Loc ortho1 = cone2.GetLoc() * -1;

                //get vector orthogonal2 to second cone line ( aka, first cone line)
                Loc ortho2 = cone1.GetLoc();

                //get dot product of diff to orthogonal1; must be less than or equal to 0
                int dot1 = Loc.Dot(diff, ortho1);

                //get dot product of diff to orthogonal2; must be greater than or equal to 0
                int dot2 = Loc.Dot(diff, ortho2);

                if (dot1 > 0 || dot2 < 0)
                    return false;
            }
            else if (limit == AreaLimit.Sides)
            {
                Loc diff = loc - origin;
                if (dir.IsDiagonal())
                {
                    //get cone vectors
                    Dir8 cone1 = DirExt.AddAngles(dir.Reverse(), Dir8.DownRight);
                    Dir8 cone2 = DirExt.AddAngles(dir.Reverse(), Dir8.DownLeft);

                    //get vector orthogonal1 to first cone line (aka, -second cone line)
                    Loc ortho1 = cone2.GetLoc() * -1;

                    //get vector orthogonal2 to second cone line ( aka, first cone line)
                    Loc ortho2 = cone1.GetLoc();

                    //get dot product of diff to orthogonal1; must be less than or equal to 0
                    int dot1 = Loc.Dot(diff, ortho1);

                    //get dot product of diff to orthogonal2; must be greater than or equal to 0
                    int dot2 = Loc.Dot(diff, ortho2);

                    if (dot1 > 0 || dot2 < 0)
                        return false;

                    //additionally, both dot products cannot be a nonzero
                    if (dot1 != 0 && dot2 != 0)
                        return false;
                }
                else
                {
                    //check if it's inside the sides
                    int dot = Loc.Dot(diff, dir.GetLoc());

                    //check if the other point is EXACTLY perpendicular
                    if (dot != 0)
                        return false;
                }
            }

            return true;
        }

        //also do this for rectangle beams (use three of them)
        public static bool IsInMovingCircle(Loc loc, Loc origin1, Loc origin2, int radius)
        {
            if (origin1 == origin2)
                return (IsInCircleSquareHitbox(loc, origin1, radius, radius, AreaLimit.Full, Dir8.Down));

            //vector from origin1 to origin2
            Loc va = origin2 - origin1;

            //vector from first origin to test point
            Loc ab = loc - origin1;

            //get closest point on the line
            int a = Loc.Dot(va, ab);

            //check collision depending on where the point is projected on the line segment
            if (a < 0)
                return (ab.DistSquared() <= radius);//check first origin collision
            else if (a > Loc.Dot(va, va))
                return ((loc - origin2).DistSquared() <= radius);//check last origin collision
            else
            {
                //right-angle vector
                Loc ortho = new Loc(-va.Y, va.X);
                int dist = Loc.Dot(ortho, ab);
                return (dist <= radius && dist >= -radius);
            }
        }

        //question: can this work even with multiple hitboxes out at once?
        //at the beginning of each hitbox, they will add their callbacks in sequence
        //at the next tick, all hitboxes will have time passed
        //the game will have detected that all blocking actions are over
        //game calls last callback added, which re-adds itself
        //thus, the game does collision checks for each hitbox individually in reverse order
        //not intended behavior for ray-based attacks; need to find an alternative
        //everything else is still okay; explosions are done in expected sequence
        //hitboxes must be grouped together somehow

        //have the owner action, on hitbox creation, call begin updateloop for all hitboxes it fires,
        //a "releasehitboxes" method that takes a list of hitboxes as args
        //(note, all actions must create all their hitboxes at once, for now, to keep it simple [unknown if nonsimultaneous is "safe"])
        //adds the hitboxes to a list
        //push into NextStep a method that:
        //--goes through all the hitboxes and updates them
        //--and then pushes its own method into NextStep (before)

        //one more issue: what about precedence between multiple hitboxes?
        //a situation may occur in which hitbox A has two hits eligible at times 0 and 2,
        //while hitbox B has two hits aligible at times 1 and 3
        //with the current logic, these hits are not shuffled together into the correct order; they will happen after one another.
        //all hitboxes of an action must precalculate into the same list
        //thus one list will contain all character, item, and tile target for all hitboxes.  preferrably stored in the CharActions queue
        //keep in mind, if CharActions is to handle all of the targeting, then it would somehow need to calculate time redundantly...
        //however, also keep in mind that all hitboxes are released at the same time, so they have synchronized time.
        //the char action would then only have to keep track of time since hitbox release

        //take note: if a character, item, and tile are on the same location, they will all get hit in that order without interruption
        //therefore an alternative method would be to have CharAction pass in a priority queue to Hitbox.UpdateHitQueue as an argument (for all hitboxes),
        //and in CharAction, have it dequeue all of its contents for the right ordering

        public abstract void PreCalculateTileEmitters();
        public abstract void PreCalculateAllTargets();
        public abstract TargetHitType IsValidTileTarget(Loc loc);

        public void UpdateHitQueue(StablePriorityQueue<int, HitboxHit> hitTargets)
        {
            //when updating, the base will update the time elapsed
            //when this method is reached, hits will be delegated accordingly

            while (TilesToHit.Count > 0 && (Finished || time >= TilesToHit.FrontPriority()))
            {
                int priority = TilesToHit.FrontPriority();
                Loc tile = TilesToHit.Dequeue();

                //filter out the hitboxes that are not wanted
                TargetHitType type = IsValidTileTarget(tile);
                if (type == TargetHitType.Burst)
                    hitTargets.Enqueue(priority, new HitboxHit(tile, true));
                else if (type == TargetHitType.Tile)
                    hitTargets.Enqueue(priority, new HitboxHit(tile, false));
            }
        }

        public bool ProcessesDone()
        {
            //if (!Finished)
            //    return false;
            if (TilesToHit.Count > 0)
                return false;
            return true;
        }


        //when working with delays, there can be set max delays, and additional delays
        //max delays do not stack; the bigger delay eclipses the smaller one
        //additional delays do stack; they can happen on a max delay as long as there is no additional delay also there
        
        public virtual void Draw(SpriteBatch spriteBatch, Loc offset) { }

        public abstract Loc GetDrawLoc(Loc offset);

        public abstract Loc GetDrawSize();
    }
    
    public class SelfHitbox : Hitbox
    {
        public override bool Finished { get { return (time >= LagBehindTime); } }


        public SelfHitbox(Character user, FiniteEmitter tileEmitter, int delay)
            : base(user, user.CharLoc * GraphicsManager.TileSize, tileEmitter, delay)
        {
        }

        public override void PreCalculateTileEmitters() { }
        public override void PreCalculateAllTargets()
        {
            TilesToHit.Enqueue(LagBehindTime, User.CharLoc);
        }
        public override TargetHitType IsValidTileTarget(Loc loc)
        {
            return TargetHitType.Burst;
        }

        public override void Update(FrameTick elapsedTime)
        {
            base.Update(elapsedTime);
        }

        public override Loc GetDrawLoc(Loc offset)
        {
            return MapLoc - offset;
        }

        public override Loc GetDrawSize()
        {
            return new Loc();
        }
    }

    public class StaticHitbox : Hitbox
    {
        public override bool Finished { get { return (time >= LagBehindTime && Emitter.Finished); } }

        public Loc Origin;//in tiles
        public Alignment TargetAlignments;
        public bool HitTiles;
        public TileAlignment BurstTiles;

        public FiniteEmitter Emitter;

        public StaticHitbox(Character user, Alignment targetAlignments, bool hitTiles, TileAlignment burstTiles, Loc origin, FiniteEmitter tileEmitter, FiniteEmitter emitter, int delay)
            : base(user, origin * GraphicsManager.TileSize, tileEmitter, delay)
        {
            TargetAlignments = targetAlignments;
            HitTiles = hitTiles;
            BurstTiles = burstTiles;
            Origin = origin;
            Emitter = (FiniteEmitter)emitter.Clone();
            Emitter.SetupEmit(MapLoc, user.MapLoc, user.CharDir);
        }

        public override void PreCalculateTileEmitters()
        {
            TilesToEmit.Enqueue(0, Origin);
        }
        public override void PreCalculateAllTargets()
        {
            TilesToHit.Enqueue(LagBehindTime, Origin);
        }
        public override TargetHitType IsValidTileTarget(Loc loc)
        {
            if (DungeonScene.Instance.IsTargeted(loc, BurstTiles))
                return TargetHitType.Burst;
            foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
            {
                if (DungeonScene.Instance.IsTargeted(User, character, TargetAlignments))
                {
                    if (loc == character.CharLoc)
                        return TargetHitType.Burst;
                }
            }
            if (HitTiles)
                return TargetHitType.Tile;
            return TargetHitType.None;
        }

        public override void Update(FrameTick elapsedTime)
        {
            base.Update(elapsedTime);

            MapLoc = Origin * GraphicsManager.TileSize;

            //update emitters
            if (!Emitter.Finished)
                Emitter.Update(DungeonScene.Instance, elapsedTime);
        }

        public override void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            //need to draw the anim here, taking into account delay
        }

        public override Loc GetDrawLoc(Loc offset)
        {
            return MapLoc - offset;
        }

        public override Loc GetDrawSize()
        {
            return new Loc();
        }
    }
    
    public class CircleSquareHitbox : Hitbox
    {
        public override bool Finished { get { return (Radius >= (int)Math.Ceiling(MaxRadius * GraphicsManager.TileSize * 1.4142136) && time >= LagBehindTime && Emitter.Finished); } }

        public Loc Origin;//in tiles
        public int Radius;

        /// <summary>
        /// Maximum Radius in Tiles
        /// </summary>
        public int MaxRadius;

        /// <summary>
        /// Speed it takes from 0 to MaxRadius in Tiles Per Second
        /// </summary>
        public int Speed;
        public AreaLimit HitArea;
        public Dir8 Dir;
        public bool HitTiles;
        public TileAlignment BurstTiles;
        public Alignment TargetAlignments;

        public CircleSquareEmitter Emitter;

        public CircleSquareHitbox(Character user, Alignment targetAlignments, bool hitTiles, TileAlignment burstTiles, Loc origin, FiniteEmitter tileEmitter,
            CircleSquareEmitter emitter, int maxRadius, int speed, int delay, AreaLimit hitArea, Dir8 dir)
            : base(user, origin * GraphicsManager.TileSize, tileEmitter, delay)
        {
            TargetAlignments = targetAlignments;
            HitTiles = hitTiles;
            BurstTiles = burstTiles;
            Origin = origin;
            MaxRadius = maxRadius;
            Speed = (speed > 0) ? speed : Math.Max(1, (int)Math.Ceiling(MaxRadius * 1.4142136 * GraphicsManager.MAX_FPS));
            HitArea = hitArea;
            Dir = dir;
            Emitter = (CircleSquareEmitter)emitter.Clone();
            Emitter.SetupEmit(MapLoc, user.CharDir, HitArea, MaxRadius * GraphicsManager.TileSize + GraphicsManager.TileSize / 2, Speed * GraphicsManager.TileSize);
        }

        private int calculateTimeToHit(Loc loc)
        {
            int distance = (int)Math.Sqrt(((loc - Origin) * GraphicsManager.TileSize).DistSquared());
            return distance * GraphicsManager.MAX_FPS / (Speed * GraphicsManager.TileSize);
        }

        public override void PreCalculateTileEmitters()
        {
            preCalculateCoverage(TilesToEmit, 0);
        }
        public override void PreCalculateAllTargets()
        {
            preCalculateCoverage(TilesToHit, LagBehindTime);
        }
        private void preCalculateCoverage(StablePriorityQueue<int, Loc> tilesToHit, int delay)
        {
            Loc topLeft = Origin - new Loc(MaxRadius);
            bool[][] connectionGrid = new bool[MaxRadius * 2 + 1][];
            for (int xx = 0; xx < connectionGrid.Length; xx++)
                connectionGrid[xx] = new bool[MaxRadius * 2 + 1];

            connectionGrid[Origin.X - topLeft.X][Origin.Y - topLeft.Y] = true;
            tilesToHit.Enqueue(calculateTimeToHit(Origin) + delay, Origin);

            Loc backup = Origin;
            if (ZoneManager.Instance.CurrentMap.TileBlocked(Origin, true))
                backup += Dir.Reverse().GetLoc();

            Grid.FloodFill(new Rect(Origin - new Loc(MaxRadius), new Loc(MaxRadius * 2 + 1)),
            (Loc testLoc) =>
            {
                if (connectionGrid[testLoc.X - topLeft.X][testLoc.Y - topLeft.Y])
                    return true;
                if (!Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, testLoc))
                    return true;
                if (!IsInSquareHitbox(testLoc, Origin, MaxRadius, HitArea, Dir))
                    return true;
                if (ZoneManager.Instance.CurrentMap.TileBlocked(testLoc, true))
                    return true;

                return false;
            },
            (Loc testLoc) =>
            {
                return false;
            },
            (Loc fillLoc) =>
            {
                if (fillLoc != Origin)
                {
                    connectionGrid[fillLoc.X - topLeft.X][fillLoc.Y - topLeft.Y] = true;
                    tilesToHit.Enqueue(calculateTimeToHit(fillLoc) + delay, fillLoc);
                }
            },
            backup);
        }

        public override TargetHitType IsValidTileTarget(Loc loc)
        {
            if (DungeonScene.Instance.IsTargeted(loc, BurstTiles))
                return TargetHitType.Burst;
            foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
            {
                if (DungeonScene.Instance.IsTargeted(User, character, TargetAlignments))
                {
                    if (loc == character.CharLoc)
                        return TargetHitType.Burst;
                }
            }
            if (HitTiles)
                return TargetHitType.Tile;
            return TargetHitType.None;
        }
        
        public override void Update(FrameTick elapsedTime)
        {
            base.Update(elapsedTime);

            if (time >= LagBehindTime)
            {
                //update the size of the hitbox
                FrameTick animTime = time - LagBehindTime;
                long add = animTime.FractionOf(Speed * GraphicsManager.TileSize, GraphicsManager.MAX_FPS);
                Radius = Math.Min((int)add, (int)Math.Ceiling(MaxRadius * GraphicsManager.TileSize * 1.4142136));
            }

            //update emittings (list of particles to emit here; place in main particle processor)
            if (!Emitter.Finished)
                Emitter.Update(DungeonScene.Instance, elapsedTime);
        }

        public override Loc GetDrawLoc(Loc offset)
        {
            return MapLoc - offset;
        }

        public override Loc GetDrawSize()
        {
            return new Loc();
        }
    }

    public class CircleSweepHitbox : Hitbox
    {
        public override bool Finished { get { return (DistanceTraveled >= MaxDistance * GraphicsManager.TileSize && !(Boomerang && reverse == 1)); } }

        public Loc StartPoint;
        /// <summary>
        /// Tiles per Second
        /// </summary>
        public int Speed;
        public Dir8 Dir;
        public int DistanceTraveled;
        /// <summary>
        /// In tiles
        /// </summary>
        public int MaxDistance;
        public bool Boomerang;
        public Alignment TargetAlignments;
        public bool HitTiles;
        public bool BurstOnWall;
        private int reverse;


        public AnimData Anim;
        public int ItemAnim;
        public AttachPointEmitter Emitter;

        public CircleSweepHitbox(Character user, Alignment targetAlignments, bool hitTiles, bool burstOnWall, Loc startPoint, AnimData anim, FiniteEmitter tileEmitter,
            AttachPointEmitter emitter, int speed, int delay, Dir8 dir, int maxDistance, bool boomerang, int item)
            : base(user, startPoint * GraphicsManager.TileSize + dir.GetLoc() * GraphicsManager.TileSize / 2, tileEmitter, delay)
        {
            TargetAlignments = targetAlignments;
            HitTiles = hitTiles;
            BurstOnWall = burstOnWall;
            StartPoint = startPoint;
            Dir = dir;
            DistanceTraveled = GraphicsManager.TileSize / 2;
            MaxDistance = maxDistance;
            Boomerang = boomerang;
            Speed = (speed > 0) ? speed : Math.Max(1, (int)Math.Ceiling((double)(MaxDistance - 0.5 + (Boomerang ? MaxDistance : 0)) * GraphicsManager.MAX_FPS));
            ItemAnim = item;
            reverse = 1;
            Anim = new AnimData(anim);
            Emitter = (AttachPointEmitter)emitter.Clone();
            Emitter.SetupEmit(User, MapLoc, User.MapLoc, user.CharDir, LocHeight);
        }


        private int calculateTimeToHit(Loc loc, bool returnTrip)
        {
            if (!returnTrip)
            {
                int distance = (loc - StartPoint).Dist8() * GraphicsManager.TileSize - GraphicsManager.TileSize / 2;
                return (distance * GraphicsManager.MAX_FPS / (Speed * GraphicsManager.TileSize));
            }
            else
            {
                int distance = ((MaxDistance - 1) - (loc - StartPoint).Dist8()) * GraphicsManager.TileSize;
                return ((MaxDistance * GraphicsManager.TileSize - GraphicsManager.TileSize / 2) * GraphicsManager.MAX_FPS / (Speed * GraphicsManager.TileSize))
                    + (distance * GraphicsManager.MAX_FPS / (Speed * GraphicsManager.TileSize));
            }
        }

        public override void PreCalculateTileEmitters()
        {
            preCalculateCoverage(TilesToEmit, 0);
        }
        public override void PreCalculateAllTargets()
        {
            preCalculateCoverage(TilesToHit, LagBehindTime);
        }
        private void preCalculateCoverage(StablePriorityQueue<int, Loc> tilesToHit, int delay)
        {
            HashSet<Loc> hitTiles = new HashSet<Loc>();
            HashSet<Loc> returnTiles = new HashSet<Loc>();

            Loc candTile = StartPoint;
            for (int ii = 0; ii < MaxDistance; ii++)
            {
                candTile = candTile + Dir.GetLoc();
                tilesToHit.Enqueue(calculateTimeToHit(candTile, false) + delay, candTile);
            }
            if (Boomerang)
            {
                for (int ii = 0; ii < MaxDistance-1; ii++)
                {
                    candTile = candTile - Dir.GetLoc();
                    tilesToHit.Enqueue(calculateTimeToHit(candTile, true) + delay, candTile);
                }
            }
        }
        public override TargetHitType IsValidTileTarget(Loc loc)
        {
            //check to see who and what is hit by the current drag of old and new origin
            foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
            {
                if (DungeonScene.Instance.IsTargeted(User, character, TargetAlignments))
                {
                    if (loc == character.CharLoc)
                        return TargetHitType.Burst;
                }
            }
            if (BurstOnWall && DungeonScene.Instance.IsTargeted(loc, TileAlignment.Wall))
                return TargetHitType.Burst;
            if (HitTiles)
                return TargetHitType.Tile;
            return TargetHitType.None;
        }

        public override void Update(FrameTick elapsedTime)
        {
            base.Update(elapsedTime);

            //get the new origin of the hitbox
            long add = UpdateTick(elapsedTime, Speed);
            int addedDist = Math.Min((int)add, MaxDistance * GraphicsManager.TileSize - DistanceTraveled);
            Loc newOrigin = MapLoc + Dir.GetLoc() * (addedDist * reverse);

            //set the new origin
            MapLoc = newOrigin;
            DistanceTraveled += addedDist;


            //update animation
            Emitter.SetupEmit(User, MapLoc, User.MapLoc, Dir, LocHeight);
            //update emittings (list of particles to emit here; place in main particle processor)
            Emitter.Update(DungeonScene.Instance, elapsedTime);

            //check to see if this hitbox needs to expire
            if (DistanceTraveled >= MaxDistance * GraphicsManager.TileSize)
            {
                if (Boomerang && reverse == 1)
                {
                    reverse = -1;
                    DistanceTraveled = GraphicsManager.TileSize / 2;
                }
                //else
                //    Finished = true;
            }
        }

        private DirSheet getAnimSheet()
        {
            DirSheet sheet = null;
            if (Anim.AnimIndex == "")
            {
                if (ItemAnim < 0)
                    return null;
                sheet = GraphicsManager.GetItem(ItemAnim);
            }
            else
                sheet = GraphicsManager.GetAttackSheet(Anim.AnimIndex);
            return sheet;
        }

        public override void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            DirSheet sheet = getAnimSheet();
            if (sheet == null)
                return;
            //draw the anim associated with this attack (aka, the projectile itself)
            Loc start = GetDrawLoc(offset);
            sheet.DrawDir(spriteBatch, start.ToVector2(), Anim.GetCurrentFrame(time, sheet.TotalFrames), DirExt.AddAngles(Dir, Anim.AnimDir));
        }

        public override Loc GetDrawLoc(Loc offset)
        {
            DirSheet sheet = getAnimSheet();
            if (sheet == null)
                return MapLoc - offset;
            return new Loc(MapLoc.X + GraphicsManager.TileSize / 2 - sheet.TileWidth / 2,
                MapLoc.Y + GraphicsManager.TileSize / 2 - sheet.TileHeight / 2) - offset;
        }

        public override Loc GetDrawSize()
        {
            DirSheet sheet = getAnimSheet();
            if (sheet == null)
                return new Loc();
            return new Loc(sheet.TileWidth, sheet.TileHeight);
        }
    }

    public class BeamSweepHitbox : Hitbox
    {
        public override bool Finished { get { return (DistanceTraveled >= MaxDistance * GraphicsManager.TileSize && timeLingered >= Linger); } }

        public Loc StartPoint;
        public Loc EndPoint;
        /// <summary>
        /// Tiles per Second
        /// </summary>
        public int Speed;
        public Dir8 Dir;
        public int DistanceTraveled;
        /// <summary>
        /// In tiles.
        /// </summary>
        public int MaxDistance;
        public bool Wide;
        public int Linger;

        public BeamAnimData Anim;
        public Alignment TargetAlignments;
        public bool HitTiles;
        public bool BurstOnWall;

        private FrameTick timeLingered;

        public BeamSweepHitbox(Character user, Alignment targetAlignments, bool tileAlignments, bool burstOnWall, Loc startPoint, BeamAnimData anim, FiniteEmitter tileEmitter,
            int speed, int delay, Dir8 dir, int maxDistance, bool wide, int linger)
            : base(user, startPoint * GraphicsManager.TileSize, tileEmitter, delay)
        {
            TargetAlignments = targetAlignments;
            HitTiles = tileAlignments;
            BurstOnWall = burstOnWall;
            StartPoint = startPoint;
            EndPoint = MapLoc;
            Dir = dir;
            MaxDistance = maxDistance;
            Speed = (speed > 0) ? speed : Math.Max(1, MaxDistance * GraphicsManager.MAX_FPS);
            Wide = wide;
            Linger = linger;
            Anim = new BeamAnimData(anim);
        }

        private int calculateTimeToHit(Loc start, Loc loc)
        {
            int distance = (loc - start).Dist8() * GraphicsManager.TileSize;
            return (distance * GraphicsManager.MAX_FPS / (Speed * GraphicsManager.TileSize));
        }

        public override void PreCalculateTileEmitters()
        {
            preCalculateCoverage(TilesToEmit, 0);
        }
        public override void PreCalculateAllTargets()
        {
            preCalculateCoverage(TilesToHit, LagBehindTime);
        }
        private void preCalculateCoverage(StablePriorityQueue<int, Loc> tilesToHit, int delay)
        {
            Loc leftDiff, rightDiff;
            if (Dir.IsDiagonal())
            {
                leftDiff = DirExt.AddAngles(Dir, Dir8.UpRight).GetLoc();
                rightDiff = DirExt.AddAngles(Dir, Dir8.UpLeft).GetLoc();
            }
            else
            {
                leftDiff = DirExt.AddAngles(Dir, Dir8.Right).GetLoc();
                rightDiff = DirExt.AddAngles(Dir, Dir8.Left).GetLoc();
            }

            Loc candTile = StartPoint;
            for (int ii = 0; ii < MaxDistance; ii++)
            {
                candTile += Dir.GetLoc();

                tilesToHit.Enqueue(calculateTimeToHit(StartPoint, candTile) + delay, candTile);

                if (Wide)
                {
                    tilesToHit.Enqueue(calculateTimeToHit(StartPoint + leftDiff, candTile + leftDiff) + delay, candTile + leftDiff);
                    tilesToHit.Enqueue(calculateTimeToHit(StartPoint + rightDiff, candTile + rightDiff) + delay, candTile + rightDiff);
                }
            }
        }
        public override TargetHitType IsValidTileTarget(Loc loc)
        {
            //check to see who and what is hit by the current drag of old and new origin
            foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
            {
                if (DungeonScene.Instance.IsTargeted(User, character, TargetAlignments))
                {
                    if (loc == character.CharLoc)
                        return TargetHitType.Burst;
                }
            }
            if (BurstOnWall && DungeonScene.Instance.IsTargeted(loc, TileAlignment.Wall))
                return TargetHitType.Burst;
            if (HitTiles)
                return TargetHitType.Tile;
            return TargetHitType.None;
        }

        public override void Update(FrameTick elapsedTime)
        {
            base.Update(elapsedTime);
            //update the new endpoint of the hitbox
            long add = UpdateTick(elapsedTime, Speed);
            int addedDist = Math.Min((int)add, MaxDistance * GraphicsManager.TileSize - DistanceTraveled);
            EndPoint = EndPoint + Dir.GetLoc() * addedDist;
            DistanceTraveled += addedDist;

            //check to see if this hitbox needs to expire
            if (DistanceTraveled >= MaxDistance * GraphicsManager.TileSize)
                timeLingered += elapsedTime;
        }

        public override void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            if (Anim.AnimIndex == "")
                return;
            //draw the beam
            Loc start = MapLoc - offset + new Loc(GraphicsManager.TileSize / 2);
            GraphicsManager.GetBeam(Anim.AnimIndex).DrawBeam(spriteBatch, start.ToVector2(), Anim.GetCurrentFrame(time, GraphicsManager.GetBeam(Anim.AnimIndex).TotalFrames), DirExt.AddAngles(Dir, Anim.AnimDir), GraphicsManager.TileSize, DistanceTraveled - GraphicsManager.TileSize / 2, Color.White * ((float)Anim.Alpha / 255));
        }

        public override Loc GetDrawLoc(Loc offset)
        {
            if (Anim.AnimIndex == "")
                return MapLoc - offset;
            return MapLoc - new Loc(DistanceTraveled) - offset;
        }

        public override Loc GetDrawSize()
        {
            if (Anim.AnimIndex == "")
                return new Loc();
            return new Loc(DistanceTraveled * 2);
        }
    }

    public class ArcingHitbox : Hitbox
    {
        public override bool Finished { get { return (DistanceTraveled >= MaxDistance) && time >= LagBehindTime; } }

        public Loc StartPoint;//in tiles
        public Loc EndPoint;
        /// <summary>
        /// In Tiles Per Second
        /// </summary>
        public int Speed;
        public int DistanceTraveled;
        public int MaxDistance;

        public AnimData Anim;
        public int ItemAnim;
        public AttachPointEmitter Emitter;

        public ArcingHitbox(Character user, Loc startPoint, AnimData anim, FiniteEmitter tileEmitter, AttachPointEmitter emitter, Loc endPoint, int speed, int item, int delay)
            : base(user, startPoint * GraphicsManager.TileSize, tileEmitter, delay)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            ItemAnim = item;
            MaxDistance = (int)Math.Sqrt(((EndPoint - StartPoint) * GraphicsManager.TileSize).DistSquared());
            Speed = (speed > 0) ? speed : Math.Max(1, (int)Math.Ceiling((double)MaxDistance / GraphicsManager.TileSize * GraphicsManager.MAX_FPS));
            Anim = new AnimData(anim);
            Emitter = (AttachPointEmitter)emitter.Clone();
            Emitter.SetupEmit(User, MapLoc, User.MapLoc, user.CharDir, LocHeight);
        }

        private int calculateTimeToHit()
        {
            return MaxDistance * GraphicsManager.MAX_FPS / (Speed * GraphicsManager.TileSize);
        }

        public override void PreCalculateTileEmitters()
        {
            TilesToEmit.Enqueue(calculateTimeToHit(), EndPoint);
        }
        public override void PreCalculateAllTargets()
        {
            TilesToHit.Enqueue(calculateTimeToHit() + LagBehindTime, EndPoint);
        }

        public override TargetHitType IsValidTileTarget(Loc loc)
        {
            return TargetHitType.Burst;
        }

        public override void Update(FrameTick elapsedTime)
        {
            base.Update(elapsedTime);
            //update the position of the item in the sky
            long add = UpdateTick(elapsedTime, Speed);
            Loc diff = (EndPoint - StartPoint) * GraphicsManager.TileSize;
            DistanceTraveled += Math.Min((int)add, MaxDistance - DistanceTraveled);

            MapLoc = StartPoint * GraphicsManager.TileSize + diff * DistanceTraveled / MaxDistance;
            LocHeight = AnimMath.GetArc(MaxDistance / 2, MaxDistance, DistanceTraveled);

            //update animation
            Emitter.SetupEmit(User, MapLoc, User.MapLoc, User.CharDir, LocHeight);
            //update emittings (list of particles to emit here; place in main particle processor)
            Emitter.Update(DungeonScene.Instance, elapsedTime);
        }

        private DirSheet getAnimSheet()
        {
            DirSheet sheet = null;
            if (Anim.AnimIndex == "")
            {
                if (ItemAnim < 0)
                    return null;
                sheet = GraphicsManager.GetItem(ItemAnim);
            }
            else
                sheet = GraphicsManager.GetAttackSheet(Anim.AnimIndex);
            return sheet;
        }

        public override void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            DirSheet sheet = getAnimSheet();
            if (sheet == null)
                return;
            //draw the anim associated with this attack (aka, the arcing projectile itself)
            Loc start = GetDrawLoc(offset);
            sheet.DrawDir(spriteBatch, new Vector2(start.X, start.Y - LocHeight), Anim.GetCurrentFrame(time, sheet.TotalFrames), DirExt.AddAngles(User.CharDir, Anim.AnimDir));
        }

        public override Loc GetDrawLoc(Loc offset)
        {
            DirSheet sheet = getAnimSheet();
            if (sheet == null)
                return MapLoc - offset;
            return new Loc(MapLoc.X + GraphicsManager.TileSize / 2 - sheet.TileWidth / 2,
                MapLoc.Y + GraphicsManager.TileSize / 2 - sheet.TileHeight / 2) - offset;
        }

        public override Loc GetDrawSize()
        {
            DirSheet sheet = getAnimSheet();
            if (sheet == null)
                return new Loc();
            return new Loc(sheet.TileWidth, sheet.TileHeight);
        }
    }

    //the owner's current-action needs a list of hitboxes it owns
    public class AttachedCircleHitbox : Hitbox
    {
        public bool HitboxDone;
        public override bool Finished { get { return HitboxDone && timeLingered >= LagBehindTime; } }

        public Loc StartPoint;//in tiles
        public int Radius;
        public int MaxDistance;//in tiles
        public Dir8 Dir;
        public bool Wide;
        public Alignment TargetAlignments;
        public bool HitTiles;
        public bool BurstOnWall;
        public int DashTime;

        public AnimData Anim;
        public int AnimOffset;
        public AttachPointEmitter Emitter;

        private FrameTick timeLingered;

        //the class will be in charge of its own hitbox movement
        //however, the current-action member of its owner may terminate early and remove this hitbox prematurely.
        //the owner class will guarantee that this hitbox will cover all the right places, as well as disposing this hitbox
        public AttachedCircleHitbox(Character user, Alignment targetAlignments, bool hitTiles, bool burstOnWall, Loc startPoint, AnimData anim, int animOffset,
            FiniteEmitter tileEmitter, AttachPointEmitter emitter, int maxDistance, int dashTime, Dir8 dir, bool wide, int delay)
            : base(user, startPoint * GraphicsManager.TileSize, tileEmitter, delay)
        {
            TargetAlignments = targetAlignments;
            HitTiles = hitTiles;
            BurstOnWall = burstOnWall;
            StartPoint = startPoint;
            MaxDistance = maxDistance;
            DashTime = dashTime;
            Dir = dir;
            Wide = wide;
            Anim = new AnimData(anim);
            AnimOffset = animOffset;
            Emitter = (AttachPointEmitter)emitter.Clone();
            Emitter.SetupEmit(User, MapLoc, MapLoc, user.CharDir, LocHeight);
        }


        private int calculateTimeToHit(Loc start, Loc loc)
        {
            //needs the dash time
            int distance = (loc - start).Dist8();
            return DashTime * distance / MaxDistance;
        }

        public override void PreCalculateTileEmitters()
        {
            preCalculateCoverage(TilesToEmit, 0);
        }
        public override void PreCalculateAllTargets()
        {
            preCalculateCoverage(TilesToHit, LagBehindTime);
        }
        private void preCalculateCoverage(StablePriorityQueue<int, Loc> tilesToHit, int delay)
        {
            Loc leftDiff, rightDiff;
            if (Dir.IsDiagonal())
            {
                leftDiff = DirExt.AddAngles(Dir, Dir8.UpRight).GetLoc();
                rightDiff = DirExt.AddAngles(Dir, Dir8.UpLeft).GetLoc();
            }
            else
            {
                leftDiff = DirExt.AddAngles(Dir, Dir8.Right).GetLoc();
                rightDiff = DirExt.AddAngles(Dir, Dir8.Left).GetLoc();
            }

            Loc candTile = StartPoint;
            for (int ii = 0; ii < MaxDistance; ii++)
            {
                candTile += Dir.GetLoc();

                tilesToHit.Enqueue(calculateTimeToHit(StartPoint, candTile) + delay, candTile);
                
                if (Wide)
                {
                    tilesToHit.Enqueue(calculateTimeToHit(StartPoint + leftDiff, candTile + leftDiff) + delay, candTile + leftDiff);
                    tilesToHit.Enqueue(calculateTimeToHit(StartPoint + rightDiff, candTile + rightDiff) + delay, candTile + rightDiff);
                }
            }
        }
        public override TargetHitType IsValidTileTarget(Loc loc)
        {
            //check to see who and what is hit by the current drag of old and new origin
            foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
            {
                if (DungeonScene.Instance.IsTargeted(User, character, TargetAlignments))
                {
                    if (loc == character.CharLoc)
                        return TargetHitType.Burst;
                }
            }
            if (BurstOnWall && DungeonScene.Instance.IsTargeted(loc, TileAlignment.Wall))
                return TargetHitType.Burst;
            if (HitTiles)
                return TargetHitType.Tile;
            return TargetHitType.None;
        }

        public override void Update(FrameTick elapsedTime)
        {
            base.Update(elapsedTime);
            //get the new origin of the hitbox from the owner
            MapLoc = User.MapLoc;

            if (HitboxDone)
                timeLingered += elapsedTime;

            //update animation
            Emitter.SetupEmit(User, MapLoc, StartPoint * GraphicsManager.TileSize, Dir, LocHeight);
            //update emittings (list of particles to emit here; place in main particle processor)
            Emitter.Update(DungeonScene.Instance, elapsedTime);
        }

        public override void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            if (Anim.AnimIndex == "")
                return;
            //draw the anim associated with this attack
            Loc start = GetDrawLoc(offset) + Dir.GetLoc() * AnimOffset;
            GraphicsManager.GetAttackSheet(Anim.AnimIndex).DrawDir(spriteBatch, start.ToVector2(), Anim.GetCurrentFrame(time, GraphicsManager.GetAttackSheet(Anim.AnimIndex).TotalFrames), DirExt.AddAngles(Dir, Anim.AnimDir));
        }

        public override Loc GetDrawLoc(Loc offset)
        {
            if (Anim.AnimIndex == "")
                return MapLoc - offset;
            return new Loc(MapLoc.X + GraphicsManager.TileSize / 2 - GraphicsManager.GetAttackSheet(Anim.AnimIndex).TileWidth / 2,
                MapLoc.Y + GraphicsManager.TileSize / 2 - GraphicsManager.GetAttackSheet(Anim.AnimIndex).TileHeight / 2) - offset;
        }

        public override Loc GetDrawSize()
        {
            if (Anim.AnimIndex == "")
                return new Loc();
            return new Loc(GraphicsManager.GetAttackSheet(Anim.AnimIndex).TileWidth, GraphicsManager.GetAttackSheet(Anim.AnimIndex).TileHeight);
        }
    }
}