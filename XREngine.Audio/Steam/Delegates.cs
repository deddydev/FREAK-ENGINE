﻿using System.Runtime.InteropServices;

namespace XREngine.Audio.Steam;

/// <summary>
/// Prototype of a callback that logs a message generated by Steam Audio. This may be implemented in any suitable way,
/// such as appending to a log file, displaying a dialog box, etc. The default behavior is to print to <c>stdout</c>.
/// </summary>
/// <param name="level">The severity level of the message.</param>
/// <param name="message">The message to log.</param>
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate void IPLLogFunction(IPLLogLevel level, [MarshalAs(UnmanagedType.LPStr)] string message);

/// <summary>
/// Prototype of a callback that allocates memory. This is usually specified to let Steam Audio use a custom memory
/// allocator. The default behavior is to use the OS-dependent aligned version of <c>malloc</c>.
/// </summary>
/// <param name="size">The number of bytes to allocate.</param>
/// <param name="alignment">The alignment (in bytes) of the start address of the allocated memory.</param>
/// <returns>Pointer to the allocated block of memory, or <c>NULL</c> if allocation failed.</returns>
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate IntPtr IPLAllocateFunction(IntPtr size, IntPtr alignment);

/// <summary>
/// Prototype of a callback that frees a block of memory. This is usually specified when using a custom memory
/// allocator with Steam Audio. The default behavior is to use the OS-dependent aligned version of <c>free</c>.
/// </summary>
/// <param name="memoryBlock">Pointer to the block of memory.</param>
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate void IPLFreeFunction(IntPtr memoryBlock);

/// <summary>
/// Callback for updating the application on the progress of a function.
/// You can use this to provide the user with visual feedback, like a progress bar.
/// </summary>
/// <param name="progress">Fraction of the function work that has been completed, between 0.0 and 1.0.</param>
/// <param name="userData">Pointer to arbitrary user-specified data provided when calling the function that will call this callback.</param>
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate void IPLProgressCallback(float progress, IntPtr userData);

/// <summary>
/// Callback for calculating the closest hit along a ray.
/// Strictly speaking, the intersection is calculated with a ray _interval_ (equivalent to a line segment). Any ray
/// interval may have multiple points of intersection with scene geometry; this function must return information
/// about the point of intersection that is closest to the ray's origin.
/// </summary>
/// <param name="ray">The ray to trace.</param>
/// <param name="minDistance">The minimum distance from the origin at which an intersection may occur for it to be considered. This function must not return any intersections closer to the origin than this value.</param>
/// <param name="maxDistance">The maximum distance from the origin at which an intersection may occur for it to be considered. This function must not return any intersections farther from the origin than this value. If this value is less than or equal to <c>minDistance</c>, the function should ignore the ray, and return immediately.</param>
/// <param name="hit">[out] Information describing the ray's intersection with geometry, if any.</param>
/// <param name="userData">Pointer to a block of memory containing arbitrary data, specified during the call to <c>iplSceneCreate</c>.</param>
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate void IPLClosestHitCallback(ref IPLRay ray, float minDistance, float maxDistance, ref IPLHit hit, IntPtr userData);

/// <summary>
/// Callback for calculating whether a ray hits any geometry.
/// Strictly speaking, the intersection is calculated with a ray _interval_ (equivalent to a line segment).
/// </summary>
/// <param name="ray">The ray to trace.</param>
/// <param name="minDistance">The minimum distance from the origin at which an intersection may occur for it to be considered. This function must not return any intersections closer to the origin than this value.</param>
/// <param name="maxDistance">The maximum distance from the origin at which an intersection may occur for it to be considered. This function must not return any intersections farther from the origin than this value. If this value is less than or equal to <c>minDistance</c>, the function should ignore the ray, set <c>occluded</c> to 1, and return immediately.</param>
/// <param name="occluded">[out] An integer indicating whether the ray intersects any geometry. A value of 0 indicates no intersection, 1 indicates that an intersection exists.</param>
/// <param name="userData">Pointer to a block of memory containing arbitrary data, specified during the call to <c>iplSceneCreate</c>.</param>
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate void IPLAnyHitCallback(ref IPLRay ray, float minDistance, float maxDistance, ref byte occluded, IntPtr userData);

/// <summary>
/// Callback for calculating the closest hit along a batch of rays.
/// Strictly speaking, the intersection is calculated with a ray _interval_ (equivalent to a line segment). Any ray
/// interval may have multiple points of intersection with scene geometry; this function must return information
/// about the point of intersection that is closest to the ray's origin.
/// </summary>
/// <param name="numRays">The number of rays to trace.</param>
/// <param name="rays">Array containing the rays.</param>
/// <param name="minDistances">Array containing, for each ray, the minimum distance from the origin at which an intersection may occur for it to be considered.</param>
/// <param name="maxDistances">Array containing, for each ray, the maximum distance from the origin at which an intersection may occur for it to be considered. If, for some ray with index <c>i</c>, `maxDistances[i]` is less than `minDistances[i]`, the function should ignore the ray.</param>
/// <param name="hits">[out] Information describing each ray's intersection with geometry, if any.</param>
/// <param name="userData">Pointer to a block of memory containing arbitrary data, specified during the call to <c>iplSceneCreate</c>.</param>
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate void IPLBatchedClosestHitCallback(int numRays, IntPtr rays, IntPtr minDistances, IntPtr maxDistances, IntPtr hits, IntPtr userData);

/// <summary>
/// Callback for calculating for each ray in a batch of rays, whether the ray hits any geometry.
/// Strictly speaking, the intersection is calculated with a ray _interval_ (equivalent to a line segment).
/// </summary>
/// <param name="numRays">The number of rays to trace.</param>
/// <param name="rays">Array containing the rays.</param>
/// <param name="minDistances">Array containing, for each ray, the minimum distance from the origin at which an intersection may occur for it to be considered.</param>
/// <param name="maxDistances">Array containing, for each ray, the maximum distance from the origin at which an intersection may occur for it to be considered. If, for some ray with index <c>i</c>, `maxDistances[i]` is less than `minDistances[i]`, the function should ignore the ray and set `occluded[i]` to 1.</param>
/// <param name="occluded">[out] Array of integers indicating, for each ray, whether the ray intersects any geometry. 0 indicates no intersection, 1 indicates that an intersection exists.</param>
/// <param name="userData">Pointer to a block of memory containing arbitrary data, specified during the call to <c>iplSceneCreate</c>.</param>
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate void IPLBatchedAnyHitCallback(int numRays, IntPtr rays, IntPtr minDistances, IntPtr maxDistances, IntPtr occluded, IntPtr userData);

/// <summary>
/// Callback for calculating how much attenuation should be applied to a sound based on its distance from the listener.
/// </summary>
/// <param name="distance">The distance (in meters) between the source and the listener.</param>
/// <param name="userData">Pointer to the arbitrary data specified in the <c>IPLDistanceAttenuationModel</c>.</param>
/// <returns>The distance attenuation to apply, between <c>0</c> and <c>1</c>. <c>0</c> = the sound is not audible, <c>1</c> = the sound is as loud as it would be if it were emitted from the listener's position.</returns>
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate float IPLDistanceAttenuationCallback(float distance, IntPtr userData);

/// <summary>
/// Callback for calculating how much air absorption should be applied to a sound based on its distance from the listener.
/// </summary>
/// <param name="distance">The distance (in meters) between the source and the listener.</param>
/// <param name="band">Index of the frequency band for which to calculate air absorption. <c>0</c> = low frequencies, <c>1</c> = middle frequencies, <c>2</c> = high frequencies.</param>
/// <param name="userData">Pointer to the arbitrary data specified in the <c>IPLAirAbsorptionModel</c>.</param>
/// <returns>The air absorption to apply, between <c>0</c> and <c>1</c>. <c>0</c> = sound in the frequency band <c>band</c> is not audible, <c>1</c> = sound in the frequency band <c>band</c> is not attenuated.</returns>
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate float IPLAirAbsorptionCallback(float distance, int band, IntPtr userData);

/// <summary>
/// Callback for calculating how much to attenuate a sound based on its directivity pattern and orientation in world space.
/// </summary>
/// <param name="direction">Unit vector (in world space) pointing forwards from the source. This is the direction that the source is "pointing towards".</param>
/// <param name="userData">Pointer to the arbitrary data specified in the <c>IPLDirectivity</c>.</param>
/// <returns>The directivity value to apply, between <c>0</c> and <c>1</c>. <c>0</c> = the sound is not audible, <c>1</c> = the sound is as loud as it would be if it had a uniform (omnidirectional) directivity pattern.</returns>
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate float IPLDirectivityCallback(IPLVector3 direction, IntPtr userData);

/// <summary>
/// Callback for visualizing valid path segments during call to <c>iplSimulatorRunPathing</c>.
/// You can use this to provide the user with visual feedback, like drawing each segment of a path.
/// </summary>
/// <param name="from">Position of starting probe.</param>
/// <param name="to">Position of ending probe.</param>
/// <param name="occluded">Occlusion status of ray segment between <c>from</c> to <c>to</c>.</param>
/// <param name="userData">Pointer to arbitrary user-specified data provided when calling the function that will call this callback.</param>
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate void IPLPathingVisualizationCallback(IPLVector3 from, IPLVector3 to, IPLbool occluded, IntPtr userData);