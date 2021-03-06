var checkMap = {
    Initial:
        "The initial state of the puzzle.",
    FindSingles:
        "A cell only has one possible value.",
    FindHiddenSingles:
        "A cell has the only occurance of a possible value in its row, column or box.",
    FindLockedCandidates1:
        "A single row or column inside a box contains all occurances of a possible value.",
    FindLockedCandidates2:
        "A single box inside a row or column contains all occurances of a possible value.",
    FindNakedPairs:
        "Two cells in the same row, column or box have the same two (and only two) possible values.",
    Guess:
        "Guess a cell's value.",
    Solved:
        "Puzzle solved!"
}

function formatPossibilities(poss) {
    var text = "     \n     \n     ".split("");
    var pi = 0;
    for(var i=1;i<=9;i++) {
        if(poss[pi] == i) {
            text[i*2-2] = i;
            pi++;
        }
    }
    return $('<pre>').text(text.join(""));
}

function formatGrid(grid) {
    var table = $('<table>');
    for(var i=0;i<9;i++) {
        var row = $('<tr>').appendTo(table);
        for(var j=0;j<9;j++) {
            var id = j + i * 9;
            var td = $('<td>').addClass("cell_"+id).prop("title", "Cell "+id).appendTo(row);
            var cell = grid[id];
            if(cell.value) {
                td.addClass("value").text(cell.value);
            } else {
                td.addClass("unknown").html(formatPossibilities(cell.poss));
            }
            if(cell.changed)
                td.addClass("changed");
        }
    }
    return table;
}



function drawOutput() {
    $('#output').empty();
    try {
        var data = JSON.parse($('#input').val());

        var grids = [];
        var row = data[0];
        var grid = [];

        grids.push({
            description:row[0],
            grid:grid,
            changed:true
        });

        for(var i=0;i<81;i++) {
            var cell = row[i+1];
            if(cell instanceof Array)
                grid[i] = {
                    poss:cell
                };
            else
                grid[i] = {
                    value:cell
                };
        }

        for(var i=1;i<data.length;i++) {
            row = data[i];
            // skip if no change this row
            if(row.length <= 1)
                continue;

            // jquery doesn't want to deep copy using $.merge
            grid = JSON.parse(JSON.stringify(grid));
            for(var j=0;j<81;j++) {
                grid[j].changed = false;
            }

            grids.push({
                description:row[0],
                grid:grid,
                changed:row.length>1
            });

            for(var j=1;j<row.length;j++) {
                var pair = row[j];
                var cell = pair[1];
                var idx = pair[0];

                if(cell instanceof Array)
                    grid[idx] = {
                        poss:cell,
                        changed:true
                    };
                else
                    grid[idx] = {
                        value:cell,
                        poss:[cell],
                        changed:true
                    };
            }
        }

        /*for(var i=0;i<grids.length;i++) {
            var grid = grids[i];
            var div = $('<div>').addClass("grid");
            var h = $('<div>').addClass("heading").html(grid.description).appendTo(div);
            if(!grid.changed) {
                div.addClass("nochange")
                $('<span>').addClass("nochange").text(" (no change)").appendTo(h);
            }
            div.append(formatGrid(grid.grid));
            $('#output').append(div);
            if(i > 0)
                div.hide();
        }*/
        
        for(var i=0;i<grids.length;i++) {
            var grid = grids[i];

            var div = $('<div>').addClass("grid");
            var descr = (i==grids.length-1)?'Solved':grids[i+1].description;
            var h = $('<div>')
                .addClass("heading")
                .text(checkMap[descr])
                .appendTo(div);
            div.append(formatGrid(grid.grid));
            $('#output').append(div);
            if(i > 0)
                div.hide();
        }

    } catch(err) {
        $('#output').text(err);
    }
}

function nextGrid() {
    var curr = $('.grid:visible');
    if(curr.length) {
        var next = curr.next();
        if(next.length) {
            curr.hide();
            next.show();
        }
    }
}

function prevGrid() {
    var curr = $('.grid:visible');
    if(curr.length) {
        var prev = curr.prev();
        if(prev.length) {
            curr.hide();
            prev.show();
        }
    }
}

$(function() {
    $('#input').bind('paste', function() {
        setTimeout(drawOutput);
    });
    $('#input').keyup(function(ev) {
        if(ev.which == 13) {
            drawOutput();
        }
    });
    $('#go').click(drawOutput);

    $('#prev').click(prevGrid);
    $('#next').click(nextGrid);

    $('body').keydown(function(ev) {
        if(ev.which == 37)
            prevGrid();
        else if(ev.which == 39)
            nextGrid();
    });

    $('#input').focus();
})
