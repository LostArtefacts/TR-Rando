LO_MODE = 0
HI_MODE = 1

m_Ranges = {}
m_Ranges[1] = "Small"
m_Ranges[2] = "Medium"
m_Ranges[3] = "Large"

m_ReturnPaths = {}
m_ReturnPaths[false] = "Off"
m_ReturnPaths[true] = "On"

m_CurrentLevel = nil
m_FilterEnabled = false
m_ValidatedOnly = true
m_SelectedRange = 2
m_UseReturnPaths = true
m_SelectedKeyItem = nil
m_KeyZones = {}

m_KeyNames = {}
-- TR1
m_KeyNames[1] = {}
m_KeyNames[1][11183] = "K1 Silver Key"
m_KeyNames[1][11143] = "P1 Gold Idol"
m_KeyNames[1][12177] = "P1 Cog (above pool)"
m_KeyNames[1][12242] = "P1 Cog (bridge)"
m_KeyNames[1][12241] = "P1 Cog (temple)"
m_KeyNames[1][14315] = "K1 Neptune Key"
m_KeyNames[1][14299] = "K2 Atlas Key"
m_KeyNames[1][14290] = "K3 Damocles Key"
m_KeyNames[1][14280] = "K4 Thor Key"
m_KeyNames[1][15217] = "K1 Rusty Key"
m_KeyNames[1][16178] = "Lead Bar (fire)"
m_KeyNames[1][16157] = "Lead Bar (spikes)"
m_KeyNames[1][16166] = "Lead Bar (temple)"
m_KeyNames[1][17245] = "K1 Gold Key"
m_KeyNames[1][17208] = "K2 Silver Key (behind door)"
m_KeyNames[1][17231] = "K2 Silver Key (between doors)"
m_KeyNames[1][17295] = "K3 Rusty Key (main room)"
m_KeyNames[1][17143] = "K3 Rusty Key (near Pierre)"
m_KeyNames[1][18389] = "K1 Gold Key (Pierre)"
m_KeyNames[1][18133] = "K1 Gold Key (flip map)"
m_KeyNames[1][18277] = "K2 Rusty Key (boulders)"
m_KeyNames[1][18267] = "K2 Rusty Key (clang-clang)"
m_KeyNames[1][19193] = "K1 Sapphire Key (end)"
m_KeyNames[1][19217] = "K1 Sapphire Key (start)"
m_KeyNames[1][20213] = "K1 Sapphire Key (alt ending)"
m_KeyNames[1][20308] = "K1 Sapphire Key (start)"
m_KeyNames[1][20160] = "P1 Eye of Horus"
m_KeyNames[1][20151] = "P2 Scarab"
m_KeyNames[1][20152] = "P3 Seal of Anubis"
m_KeyNames[1][20163] = "P4 Ankh"
m_KeyNames[1][21191] = "K1 Gold Key"
m_KeyNames[1][21196] = "P1 Ankh (after key)"
m_KeyNames[1][21100] = "P1 Ankh (behind Sphinx)"
m_KeyNames[1][21202] = "P2 Scarab"
m_KeyNames[1][22137] = "K1 Rusty Key"
m_KeyNames[1][22160] = "P1 Fuse (boulders)"
m_KeyNames[1][22183] = "P1 Fuse (conveyor)"
m_KeyNames[1][22148] = "P1 Fuse (Cowboy)"
m_KeyNames[1][22146] = "P1 Fuse (Cowboy, alt)"
m_KeyNames[1][22216] = "P2 Pyramid Key"

-- TR2
m_KeyNames[2] = {}
m_KeyNames[2][10291] = "K1 Guardhouse Key"
m_KeyNames[2][10250] = "K2 Rusty Key"
m_KeyNames[2][11210] = "K1 Boathouse Key"
m_KeyNames[2][11353] = "K2 Steel Key"
m_KeyNames[2][11502] = "K3 Iron Key"
m_KeyNames[2][12241] = "K1 Library Key"
m_KeyNames[2][12385] = "K2 Detonator Key"
m_KeyNames[2][13449] = "K1 Ornate Key (fan area)"
m_KeyNames[2][13544] = "K1 Ornate Key (start)"
m_KeyNames[2][13395] = "P1 Relay Box"
m_KeyNames[2][13377] = "P3 Circuit Board"
m_KeyNames[2][14238] = "K1 Red Pass Card"
m_KeyNames[2][14193] = "K2 Yellow Pass Card"
m_KeyNames[2][14240] = "K3 Green Pass Card"
m_KeyNames[2][15250] = "K1 Red Pass Card"
m_KeyNames[2][15301] = "K4 Blue Pass Card"
m_KeyNames[2][15299] = "P1 Machine Chip (helicopter area)"
m_KeyNames[2][15206] = "P1 Machine Chip (middle room)"
m_KeyNames[2][17407] = "K1 Rest Room Key"
m_KeyNames[2][17343] = "K2 Rusty Key"
m_KeyNames[2][17324] = "K3 Cabin Key"
m_KeyNames[2][17298] = "P1 Circuit Breaker (rest room area)"
m_KeyNames[2][17349] = "P1 Circuit Breaker (shard room)"
m_KeyNames[2][17304] = "P1 Circuit Breaker (staircase)"
m_KeyNames[2][18300] = "K1 Theatre Key"
m_KeyNames[2][19368] = "K2 Stern Key"
m_KeyNames[2][19385] = "K3 Storage Key"
m_KeyNames[2][19262] = "K4 Cabin Key"
m_KeyNames[2][19400] = "P4 The Seraph"
m_KeyNames[2][20379] = "K1 Drawbridge Key"
m_KeyNames[2][20348] = "K2 Hut Key"
m_KeyNames[2][21328] = "K1 Strongroom Key"
m_KeyNames[2][21501] = "K2 Trapdoor Key"
m_KeyNames[2][21225] = "K3 Rooftops Key"
m_KeyNames[2][21464] = "K4 Main Hall Key"
m_KeyNames[2][21306] = "P1 Prayer Wheel (burner room)"
m_KeyNames[2][21415] = "P1 Prayer Wheel (ladder tower)"
m_KeyNames[2][21431] = "P1 Prayer Wheel (outside area)"
m_KeyNames[2][21485] = "P1 Prayer Wheel (pool area)"
m_KeyNames[2][21330] = "P1 Prayer Wheel (rooftops)"
m_KeyNames[2][21338] = "P3 Gemstone (east)"
m_KeyNames[2][21332] = "P3 Gemstone (west)"
m_KeyNames[2][21273] = "P4 The Seraph"
m_KeyNames[2][22222] = "P1 Tibetan Mask (start)"
m_KeyNames[2][22357] = "P1 Tibetan Mask (underwater)"
m_KeyNames[2][22342] = "Q1 Gong Hammer (easter egg)"
m_KeyNames[2][23315] = "K2 Gong Hammer"
m_KeyNames[2][23379] = "P1 Tibetan Mask"
m_KeyNames[2][23503] = "Q2 Talion"
m_KeyNames[2][24427] = "K2 Gold Key"
m_KeyNames[2][24582] = "K3 Silver Key"
m_KeyNames[2][24436] = "K4 Main Chamber Key"
m_KeyNames[2][24387] = "P1 The Dragon Seal"
m_KeyNames[2][25443] = "P1 Mystic Plaque (alt ending)"
m_KeyNames[2][25232] = "P1 Mystic Plaque (west)"
m_KeyNames[2][25160] = "P3 Mystic Plaque (east)"
m_KeyNames[2][26145] = "P1 Mystic Plaque"

-- TR3
m_KeyNames[3] = {}
m_KeyNames[3][10590] = "K4 Indra Key"
m_KeyNames[3][11723] = "K1 Key of Ganesha (current pool)"
m_KeyNames[3][11641] = "K1 Key of Ganesha (flipmap pool)"
m_KeyNames[3][11539] = "K1 Key of Ganesha (mudslide)"
m_KeyNames[3][11708] = "K1 Key of Ganesha (Randy/Rory)"
m_KeyNames[3][11692] = "K1 Key of Ganesha (spike ceiling)"
m_KeyNames[3][11636] = "P1 Scimitar (east)"
m_KeyNames[3][11648] = "P2 Scimitar (west)"
m_KeyNames[3][12425] = "K1 Gate Key (monkey pit)"
m_KeyNames[3][12258] = "K1 Gate Key (snake pit)"
m_KeyNames[3][14300] = "K1 Smuggler's Key"
m_KeyNames[3][14447] = "P1 Serpent Stone (above pool)"
m_KeyNames[3][14440] = "P1 Serpent Stone (treetops)"
m_KeyNames[3][14252] = "P1 Serpent Stone (waterfall)"
m_KeyNames[3][15364] = "K1 Commander Bishop's Key"
m_KeyNames[3][15278] = "K2 Lt. Tuckerman's Key"
m_KeyNames[3][18524] = "K1 Flue Room Key"
m_KeyNames[3][18161] = "K2 Cathedral Key"
m_KeyNames[3][19279] = "K1 Maintenance Key"
m_KeyNames[3][19362] = "K2 Solomon's Key (door puzzle)"
m_KeyNames[3][19239] = "K3 Solomon's Key (drill shaft)"
m_KeyNames[3][19343] = "P1 Old Coin"
m_KeyNames[3][19302] = "P2 Ticket"
m_KeyNames[3][19465] = "P3 Hammer"
m_KeyNames[3][19448] = "P4 Ornate Star"
m_KeyNames[3][20447] = "K1 Boiler Room Key"
m_KeyNames[3][20240] = "P1 Embalming Fluid"
m_KeyNames[3][22578] = "K1 Generator Access Card"
m_KeyNames[3][22359] = "K2 Detonator Key"
m_KeyNames[3][22419] = "K2 Detonator Key (unused)"
m_KeyNames[3][23260] = "K1 Keycard Type A"
m_KeyNames[3][23490] = "K2 Keycard Type B (satellite dish tower)"
m_KeyNames[3][23454] = "K2 Keycard Type B (turret area)"
m_KeyNames[3][23352] = "P1 Blue Security Pass"
m_KeyNames[3][23322] = "P2 Yellow Security Pass (end)"
m_KeyNames[3][23537] = "P2 Yellow Security Pass (hangar access)"
m_KeyNames[3][23463] = "P2 Yellow Security Pass (satellite dish tower)"
m_KeyNames[3][24433] = "K1 Launch Code Card"
m_KeyNames[3][24322] = "P2 Code CD (silo)"
m_KeyNames[3][24253] = "P3 Code CD (watchtower)"
m_KeyNames[3][24434] = "P4 Hangar Access Pass"
m_KeyNames[3][25526] = "K1 Hut Key"
m_KeyNames[3][25374] = "P1 Crowbar (gate control)"
m_KeyNames[3][25456] = "P1 Crowbar (regular)"
m_KeyNames[3][25371] = "P1 Crowbar (tower)"
m_KeyNames[3][25446] = "P2 Gate Control Key"
m_KeyNames[3][26679] = "P1 Crowbar"
m_KeyNames[3][26444] = "P2 Lead Acid Battery"
m_KeyNames[3][26509] = "P3 Winch Starter"
m_KeyNames[3][27686] = "K1 Uli Key (end)"
m_KeyNames[3][27269] = "K1 Uli Key (start)"
m_KeyNames[3][27421] = "P1 Oceanic Mask (earth)"
m_KeyNames[3][27449] = "P1 Oceanic Mask (fire)"
m_KeyNames[3][27395] = "P1 Oceanic Mask (water)"
m_KeyNames[3][27368] = "P1 Oceanic Mask (wind)"
m_KeyNames[3][29220] = "K1 Vault Key"

function select_key_item(id)
    m_SelectedKeyItem = id
    if m_FilterEnabled then
        select_zones()
        update_filter()
    end
end

function select_quick_view(range, return_paths)
    m_SelectedRange = range
    m_UseReturnPaths = return_paths
    
    select_zones()    
    if m_FilterEnabled then
        update_filter()
    end
end

function select_zones()
    if not m_FilterEnabled or m_SelectedKeyItem == nil then
        return
    end
    
    local matches = 0
    for _, zone in pairs(m_KeyZones[m_SelectedKeyItem]) do
        local wp = trview.route.waypoints[zone.low]
        zone.selected = wp.randomizer_settings.Range == m_Ranges[m_SelectedRange]
            and wp.randomizer_settings.RequiresReturnPath == m_UseReturnPaths
        if zone.selected then
            matches = matches + 1
        end
    end

    if matches == 0 then
        for _, zone in pairs(m_KeyZones[m_SelectedKeyItem]) do
            zone.selected = true
            break
        end
    end
end

function select_zone(zone)
    for _, z in pairs(m_KeyZones[m_SelectedKeyItem]) do
        z.selected = z == zone
        if z.selected then
            local wp = trview.route.waypoints[zone.low]
            for range_id, range_desc in pairs(m_Ranges) do
                if range_desc == wp.randomizer_settings.Range then
                    m_SelectedRange = range_id
                end
            end
            m_UseReturnPaths = wp.randomizer_settings.RequiresReturnPath
        end
    end
    
    if m_FilterEnabled then
        update_filter()
    end
end

function show_wp_room(idx, low_idx)
    local wp = trview.route.waypoints[idx]
    local low_wp = trview.route.waypoints[low_idx]
    if (m_ValidatedOnly and not wp.randomizer_settings.Validated)
    or (wp.randomizer_settings.RoomType == "ReturnPath" and not low_wp.randomizer_settings.RequiresReturnPath) then
        return
    end
    
    local room = trview.level.rooms[wp.room_number + 1]
    if room ~= nil then
        room.visible = true
    end
end

function update_filter()
    if trview.level == nil then return end

    for _, r in pairs(trview.level.rooms) do
        r.visible = not m_FilterEnabled
    end
    
    if not m_FilterEnabled or m_SelectedKeyItem == nil then
        return
    end
    
    for _, zone in pairs(m_KeyZones[m_SelectedKeyItem]) do
        if zone.selected then
            local high = zone.high
            if high == -1 then
                -- Zone extends to EOL
                high = #trview.route.waypoints
            else
                local wp = trview.route.waypoints[high]
                local exc_room = wp.room_number
                while wp.room_number == exc_room and high > 0 do
                    high = high - 1
                    wp = trview.route.waypoints[high]
                end
            end
        
            for i = zone.low, high, 1 do
                show_wp_room(i, zone.low)
            end
        end
    end
end

function show_unzoned_rooms()
    if trview.level == nil then return end
    
    for _, r in pairs(trview.level.rooms) do
        r.visible = true
    end
    
    for _, wp in pairs(trview.route.waypoints) do
        local room = trview.level.rooms[wp.room_number + 1]
        if room ~= nil then
            room.visible = false
        end
    end
end
    
function update_key_items()
    if trview.level == nil or trview.level.version > 3 then
        return
    end
    
    m_KeyZones = {}
    
    for _, wp in pairs(trview.route.waypoints) do
        for id in string.gmatch(wp.randomizer_settings.KeyItemsLow, '([^,]+)') do
            local key_id = tonumber(id)
            m_KeyZones[key_id] = {}

            for _, range in pairs(m_Ranges) do
                for paths, _ in pairs(m_ReturnPaths) do
                    build_key_zones(key_id, range, paths)
                end
            end
        end
    end
end

function ordered_key_items()
    local keys = {}
    for k in pairs(m_KeyZones) do
        keys[#keys+1] = k
    end

    table.sort(keys, function(a,b)
        return string.lower(get_key_name(a)) < string.lower(get_key_name(b))
    end)

    local i = 0
    return function()
        i = i + 1
        if keys[i] then
            return keys[i], m_KeyZones[keys[i]]
        end
    end
end

function build_key_zones(key_id, range, paths)
    local zone = {}
    for idx, wp in pairs(trview.route.waypoints) do
        if test_waypoint(wp, LO_MODE, key_id, range, paths) then
            zone = {}
            zone.low = idx
            zone.high = -1
            zone.selected = false
            table.insert(m_KeyZones[key_id], zone)
        end
       
        if test_waypoint(wp, HI_MODE, key_id, range, paths) then
            zone.high = idx
        end
    end
end

function test_waypoint(wp, mode, key_id, range, paths)
    local key_ids = nil
    if mode == LO_MODE then
        key_ids = wp.randomizer_settings.KeyItemsLow
    else
        key_ids = wp.randomizer_settings.KeyItemsHigh
    end
    
    if not string.find(key_ids, tostring(key_id)) then
        return false
    end
    
    if wp.randomizer_settings.Range ~= range then
        return false
    end
    
    return (wp.randomizer_settings.RequiresReturnPath and paths)
        or (not wp.randomizer_settings.RequiresReturnPath and not paths)
end

function get_quick_label(range_id, return_path_id)
    return m_Ranges[range_id] .. "/" .. m_ReturnPaths[return_path_id]
end

function get_key_name(id)
    if m_KeyNames[trview.level.version][id] ~= nil then
        return m_KeyNames[trview.level.version][id]
    end
    return "Unknown"
end

function get_high_wp_name(zone)
    if zone.high < 0 then
        return "N/A"
    end
    return tostring(zone.high - 1)
end

function refresh()
    if m_CurrentLevel ~= trview.level.filename then
        m_FilterEnabled = false
        m_SelectedKeyItem = nil
        update_key_items()
        m_CurrentLevel = trview.level.filename
    end
end

function render_ui()
    if trview.level == nil or trview.level.version > 3 then
        return
    end

    refresh()

    if (ImGui.Begin { name = "Key Item Zones", flags = ImGui.WindowFlags.AlwaysAutoResize }) then        
        local filter_changed = false
        filter_changed, m_FilterEnabled = ImGui.Checkbox({ label = "Show zoning", checked = m_FilterEnabled })

        local valid_changed = false
        ImGui.SameLine()
        valid_changed, m_ValidatedOnly = ImGui.Checkbox({ label = "Validated", checked = m_ValidatedOnly })
        
        ImGui.SameLine()
        if ImGui.Button({ label = "Unzoned" }) then
            show_unzoned_rooms()
        end
        
        ImGui.SameLine()
        if ImGui.Button({ label = "Refresh" }) then
            update_key_items()
        end
                
        if (filter_changed or valid_changed) then
            select_zones()
            update_filter()
        end 

        if ImGui.BeginCombo({ label = "Quick View", preview_value = get_quick_label(m_SelectedRange, m_UseReturnPaths) }) then
            for range_id, _ in pairs(m_Ranges) do
                for rp_id, _ in pairs(m_ReturnPaths) do
                    if (ImGui.Selectable(
                    { 
                        label = get_quick_label(range_id, rp_id),
                        selected = range_id == m_SelectedRange and rp_id == m_UseReturnPaths,
                        flags = ImGui.SelectableFlags.SelectOnNav
                    })) then
                        select_quick_view(range_id, rp_id)
                    end
                end
            end
            ImGui.EndCombo()
        end

        if ImGui.BeginTable({ id = "Key Items", column = 2 }) then 
            ImGui.TableSetupColumn({ label = "Key Item" })
            ImGui.TableSetupColumn({ label = "Description" })
            ImGui.TableSetupScrollFreeze({ cols = 1, rows = 1 })
            ImGui.TableHeadersRow()

            for id, _ in ordered_key_items() do
                ImGui.TableNextColumn()
                if (ImGui.Selectable(
                    { 
                        label = tostring(id),
                        selected = id == m_SelectedKeyItem,
                        flags = ImGui.SelectableFlags.SpanAllColumns | ImGui.SelectableFlags.SelectOnNav
                    })) then
                    select_key_item(id)
                end
                ImGui.TableNextColumn()
                ImGui.Text({text = get_key_name(id)})

            end
            ImGui.EndTable()
        end
        
        if m_SelectedKeyItem ~= nil and ImGui.BeginTable({ id = "Zones", column = 4 }) then 
            ImGui.TableSetupColumn({ label = "Low WP" })
            ImGui.TableSetupColumn({ label = "High WP" })
            ImGui.TableSetupColumn({ label = "Range" })
            ImGui.TableSetupColumn({ label = "Return Paths" })
            ImGui.TableSetupScrollFreeze({ cols = 1, rows = 1 })
            ImGui.TableHeadersRow()
            
            for _, zone in pairs(m_KeyZones[m_SelectedKeyItem]) do
                local wp = trview.route.waypoints[zone.low]
                ImGui.TableNextColumn()
                if (ImGui.Selectable(
                    { 
                        label = tostring(zone.low - 1),
                        selected = zone.selected,
                        flags = ImGui.SelectableFlags.SpanAllColumns | ImGui.SelectableFlags.SelectOnNav
                    })) then
                    select_zone(zone)
                end
                ImGui.TableNextColumn()
                ImGui.Text({ text = get_high_wp_name(zone) })
                ImGui.TableNextColumn()
                ImGui.Text({ text = tostring(wp.randomizer_settings.Range) })
                ImGui.TableNextColumn()
                ImGui.Text({ text = m_ReturnPaths[wp.randomizer_settings.RequiresReturnPath] })
            end
            ImGui.EndTable()
        end
    end
    ImGui.End()
end

update_key_items()
print("Rando key item zoning loaded")
