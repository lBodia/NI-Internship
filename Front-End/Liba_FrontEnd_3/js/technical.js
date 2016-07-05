/**
 * Find the next element matching a certain selector. Differs from next() in
 *  that it searches outside the current element's parent.
 *  
 * @param selector The selector to search for
 * @param steps (optional) The number of steps to search, the default is 1
 * @param scope (optional) The scope to search in, the default is document wide 
 */
$.fn.findNext = function(selector, steps, scope)
{
    // Steps given? Then parse to int 
    if (steps)
    {
        steps = Math.floor(steps);
    }
    else if (steps === 0)
    {
        // Stupid case :)
        return this;
    }
    else
    {
        // Else, try the easy way
        var next = this.next(selector);
        if (next.length)
            return next;
        // Easy way failed, try the hard way :)
        steps = 1;
    }

    // Set scope to document or user-defined
    scope = (scope) ? $(scope) : $(document);

    // Find kids that match selector: used as exclusion filter
    var kids = this.find(selector);

    // Find in parent(s)
    hay = $(this);
    while(hay[0] != scope[0])
    {
        // Move up one level
        hay = hay.parent();     
        // Select all kids of parent
        //  - excluding kids of current element (next != inside),
        //  - add current element (will be added in document order)
        var rs = hay.find(selector).not(kids).add($(this));
        // Move the desired number of steps
        var id = rs.index(this) + steps;
        // Result found? then return
        if (id > -1 && id < rs.length)
            return $(rs[id]);
    }
    // Return empty result
    return $([]);
}
