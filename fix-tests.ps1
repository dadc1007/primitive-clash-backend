# Script to fix all test files

$files = @(
    "PrimitiveClash.Backend.Tests\Utils\Mappers\DeckMapperTests.cs",
    "PrimitiveClash.Backend.Tests\Utils\Mappers\RefreshHandNotificationMapperTests.cs",
    "PrimitiveClash.Backend.Tests\Utils\Mappers\PlayerHandNotificationMapperTests.cs",
    "PrimitiveClash.Backend.Tests\Utils\Mappers\JoinedToGameNotificationMapperTests.cs",
    "PrimitiveClash.Backend.Tests\Utils\Mappers\CardMapperTests.cs"
)

foreach ($file in $files) {
    $content = Get-Content $file -Raw
    
    # Fix Arena constructor and ArenaTemplate properties
    $content = $content -replace 'Height = 32,\s*Width = 18', 'Name = "Test Arena"'
    $content = $content -replace 'var arena = new Arena\(arenaTemplate\);', 'var towers = new Dictionary<Guid, List<Tower>>();
        var arena = new Arena(arenaTemplate, towers);'
    
    # Fix Game constructor
    $content = $content -replace 'var game = new Game\s*\{\s*Id = ([^,]+),\s*State = ([^,]+),\s*PlayerStates = ([^,]+),\s*GameArena = ([^\}]+)\s*\};', 'var game = new Game($1, $3, $4)
        {
            State = $2
        };'
    
    # Fix BuildingCard Lifetime to Duration
    $content = $content -replace 'Lifetime = (\d+)', 'Duration = $1'
    
    # Add Targets property to all TroopCard that don't have it
    $pattern = '(new TroopCard\s*\{[^}]*Name = "[^"]+",\s*ElixirCost = \d+,\s*ImageUrl = "[^"]*")\s*\}'
    $replacement = '$1,
            Targets = new List<UnitClass> { UnitClass.Ground }
        }'
    $content = $content -replace $pattern, $replacement
    
    Set-Content $file $content -NoNewline
    Write-Host "Fixed $file"
}

Write-Host "All files fixed"
