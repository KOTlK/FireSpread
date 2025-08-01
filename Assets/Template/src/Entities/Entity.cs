using UnityEngine;
using System;
using System.Runtime.CompilerServices;

[Flags]
public enum EntityFlags
{
    None            = 0,
    Dynamic         = 1 << 1,
    Static          = 1 << 2,
    InsideHashTable = 1 << 3,
    UpdatePhysics   = 1 << 4,
}

public enum EntityType {
    Plant
}

public class Entity : MonoBehaviour, ISave  {
    public string        Name;
    public EntityHandle  Handle;
    public EntityFlags   Flags;
    public EntityType    Type;
    public EntityManager Em;
    public World         World;
    public bool          AutoBake;

    public Vector3 Position {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => transform.position;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => transform.position = value;
    }

    public Quaternion Rotation {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => transform.rotation;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => transform.rotation = value;
    }

    public Vector3 Scale {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => transform.localScale;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => transform.localScale = value;
    }

    public Matrix4x4 ObjectToWorld {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => transform.localToWorldMatrix;
    }

    private void Awake() {
        if(AutoBake) {
            if(Singleton<EntityManager>.Exist) {
                Singleton<EntityManager>.Instance.BakeEntity(this);
            }
        }
    }

    public virtual void RegisterInstanceId(EntityManager em) {
        em.EntityByInstanceId.Add(gameObject.GetInstanceID(), Handle);
    }

    public virtual void UnRegisterInstanceId(EntityManager em) {
        em.EntityByInstanceId.Remove(gameObject.GetInstanceID());
    }

    public virtual void OnBaking(){ }
    public virtual void OnCreate(){ }
    public virtual void Execute(){ }
    public virtual void UpdatePhysics(){ }

    public virtual void Destroy() {
        Destroy(gameObject);
    }

    public virtual void Save(ISaveFile sf) {
    }

    public virtual void Load(ISaveFile sf) {
    }

    public void MoveEntity(Vector3 position) {
        transform.position = position;
        Em.MovedEntities.Add(new MovedEntity{
            Id = Handle.Id,
            NewPosition = position
        });
    }

    public void MoveEntity(Vector3 position, Quaternion rotation) {
        transform.SetPositionAndRotation(position, rotation);
        Em.MovedEntities.Add(new MovedEntity{
            Id = Handle.Id,
            NewPosition = transform.position
        });
    }

    public (Vector3 velocity, int collisionCount)
            MovePhysicsEntityNoGravity(Vector3 initialVelocity,
                                       Quaternion   rotation,
                                       float        radius,
                                       RaycastHit[] hitBuffer,
                                       float        skinWidth           = 0.01f,
                                       int          maxIterationCount   = 8) {
        var velocity            = initialVelocity;
        var frameVelocity       = initialVelocity * Clock.Delta;
        var collisionCount      = 0;
        var initialPosition     = transform.position;
        var position            = initialPosition;
        var bufferLength        = hitBuffer.Length;
        var velocityLeft        = frameVelocity.magnitude;
        var direction           = frameVelocity.normalized;
        RaycastHit hit;

        for(var i = 0; i < maxIterationCount; ++i) {
            if(Physics.SphereCast(position, radius, direction, out hit, velocityLeft)) {
                velocityLeft    -= hit.distance + skinWidth;
                position        += frameVelocity.normalized * (hit.distance - skinWidth);
                frameVelocity   = Vector3.ProjectOnPlane(frameVelocity, hit.normal).normalized
                                * velocityLeft;

                if(collisionCount >= bufferLength) {
                    velocity = Vector3.Reflect((position - initialPosition).normalized, hit.normal) * initialVelocity.magnitude / 2;
                    break;
                } else {
                    var addCollision = true;

                    for(var j = 0; j < collisionCount; ++j) {
                        if(hitBuffer[j].colliderInstanceID == hit.colliderInstanceID) {
                            addCollision = false;
                            break;
                        }
                    }

                    if(addCollision) {
                        hitBuffer[collisionCount++] = hit;
                    }
                }

                if(velocityLeft <= skinWidth) {
                    velocity = Vector3.Reflect((position - initialPosition).normalized, hit.normal) * initialVelocity.magnitude / 2;
                    break;
                }
            } else {
                position += frameVelocity;
                break;
            }
        }

        MoveEntity(position, rotation);
        return (velocity, collisionCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint QueryNearbyEntities(float radius, uint[] buffer, bool includeStatic = true) {
        return World.QueryNearbyEntities(transform.position, buffer, radius, includeStatic);
    }

    // Helper methods to get and destroy entities without calling Em...
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetEntity(EntityHandle handle, out Entity e) {
        return Em.GetEntity(handle, out e);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetEntity<T>(EntityHandle handle, out T e)
    where T : Entity {
        return Em.GetEntity<T>(handle, out e);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DestroyThisEntity() {
        Em.DestroyEntity(Handle);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DestroyEntity(EntityHandle handle) {
        Em.DestroyEntity(handle);
    }
}