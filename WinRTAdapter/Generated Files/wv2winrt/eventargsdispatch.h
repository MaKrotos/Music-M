// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#pragma once
#include <windows.h>

namespace wv2winrt_impl
{
// Creates an aggregate IDispatch object that combines the provided
// winrt event event args and source type in the following manner:
//  * root - The root object is a modified version of the WinRT event args
//      * target - The target property is the source of the event
//      * detail - An array in which 0 is the unmodified WinRT event args, and the rest are all
//      the other params to invoke
//      * type - The name of the event
//
// This also supports the case where there is no source or the source is
// null or the event args are null. In the case that there is no source
// the target will still exist but will be null.
// In the case that the event args are null, the detail array will still
// exist but its 0 index will be null.
HRESULT CreateAggregateEventArgs(
    LPCWSTR eventName, IDispatch* source, VARIANT* unmodifiedDelegateParamsAsVARIANT,
    const ULONG unmodifiedDelegateParamsAsVARIANTCount, IDispatch** aggregateEventArgs);

} // namespace wv2winrt_impl
