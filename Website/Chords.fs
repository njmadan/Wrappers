﻿[<JS>]
module Website.Chords

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
type d3 = Wrappers.D3.d3

type TickAngle =
    {
        angle: float
        label: string
    }

type ChordGroup =
    {
        index: int
        startAngle: float
        endAngle: float
        value: float
    }

let run () =
    let matrix =
        [|
            [|11975;  5871; 8916; 2868|]
            [| 1951; 10048; 2060; 6171|]
            [| 8010; 16145; 8090; 8045|]
            [| 1013;   990;  940; 6907|]
        |]

    let chord =
        d3.layout
            .chord()
            .padding(0.05)
            .sortSubgroups(d3.descending)
            .matrix matrix

    let width = 960.0
    let height = 500.0
    let innerRadius = min width height * 0.41
    let outerRadius = innerRadius * 1.1

    let fill =
        d3.scale
            .ordinal()
            .domain([|0..3|])
            .range [|"#000000"; "#FFDD89"; "#957244"; "#F26223"|]

    let svg =
        d3.select("body")
            .append("svg")
            .attr("width", width)
            .attr("height", height)
            .append("g")
            .attr("transform", "translate(" + string (width / 2.0) + "," + string (height / 2.0) + ")")

    let fade (opacity: float) (g: obj, i: int) =
        svg.selectAll(".chord path")
            .filter(fun d -> d?source?index <> i && d?target?index <> i)
            .transition()
            .style("opacity", opacity)
        |> ignore
        
    svg.append("g")
        .selectAll("path")
        .data(chord.groups)
        .enter().append("path")
        .style("fill", fun d -> fill d.index)
        .style("stroke", fun d -> fill d.index)
        .attr("d", d3.svg.arc().innerRadius(innerRadius).outerRadius outerRadius)
        .on("mouseover", fade 0.1)
        .on("mouseout", fade 1.0)
    |> ignore

    let groupTicks d =
        let k = (d.endAngle - d.startAngle) / d.value
        [|0..1000..int d.value|]
        |> Array.mapi (fun i v ->
                {
                    angle = float v * k + d.startAngle
                    label = if i % 5 = 0 then string (v / 1000) + "k" else null
                }
            )

    let ticks =
        svg.append("g")
            .selectAll("g")
            .data(chord.groups)
            .enter().append("g").selectAll("g")
            .data(groupTicks)
            .enter().append("g")
            .attr("transform", fun d -> "rotate(" + string (d.angle * 18e1 / System.Math.PI - 9e1) + ") translate(" + string outerRadius + ",0)")

    ticks.append("line")
        .attr("x1", 1)
        .attr("y1", 0)
        .attr("x2", 5)
        .attr("y2", 0)
        .style("stroke", "#000")
    |> ignore

    ticks.append("text")
        .attr("x", 8)
        .attr("dy", ".35em")
        .attr("transform", fun d -> if d.angle > System.Math.PI then "rotate(180) translate(-16)" else null)
        .style("text-anchor", fun d -> if d.angle > System.Math.PI then "end" else null)
        .text(fun d -> d.label)
    |> ignore

    svg.append("g")
        .attr("class", "chord")
        .selectAll("path")
        .data(chord.chords)
        .enter().append("path")
        .attr("d", d3.svg.chord().radius innerRadius)
        .style("fill", fun d -> fill d?target?index)
        .style("opacity", 1)
    |> ignore

let body() =
    run()
    Div [
        A [HRef "https://github.com/davidgrenier/Wrappers/blob/master/Website/Chords.fs"; Text "Source"]
    ]